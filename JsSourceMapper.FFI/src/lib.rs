extern crate js_source_mapper;
extern crate libc;

use js_source_mapper as jsm;
use libc::{c_char, uint32_t};
use std::ffi::{CStr, CString};
use std::str;
use std::ptr;

#[no_mangle]
pub struct Cache {
  /// Internal result, not exposed to outside world
  result: Result<jsm::Cache, String>
}

#[no_mangle]
#[repr(C)]
pub struct Mapping {
  source_line: uint32_t,
  source_column: uint32_t,
  generated_line: uint32_t,
  generated_column: uint32_t,
  source: *const c_char,
  name: *const c_char
}

#[no_mangle]
pub extern fn cache_init(s: *const c_char) -> *mut Cache {
  Box::into_raw(Box::new(Cache { result: internal_consume(s) }))
}

#[no_mangle]
pub extern fn cache_free(ptr: *mut Cache) {
    if ptr.is_null() { return }
    unsafe { Box::from_raw(ptr); }
}

#[no_mangle]
pub extern fn find_mapping(cache_ptr: *const Cache, line: uint32_t, column: uint32_t) -> *mut Mapping {
  let cache = unsafe {
    if cache_ptr.is_null() { return ptr::null_mut() }
    match &(&*cache_ptr).result {
      &Ok(ref c) => c,
      &Err(_) => return ptr::null_mut()
    }
  };
  let mapping = cache.mapping_for_generated_position(line as u32, column as u32);
  Box::into_raw(Box::new(Mapping {
    source_line: mapping.original.line as uint32_t,
    source_column: mapping.original.column as uint32_t,
    generated_line: mapping.generated.line as uint32_t,
    generated_column: mapping.generated.column as uint32_t,
    source: CString::new(mapping.source).unwrap().into_raw(),
    name: CString::new(mapping.name).unwrap().into_raw()
  }))
}

#[no_mangle]
pub extern fn mapping_free(mapping_ptr: *mut Mapping) {
  if mapping_ptr.is_null() { return }
  unsafe {
    let mut mapping = *Box::from_raw(mapping_ptr);
    let source = std::mem::replace(&mut mapping.source, ptr::null()) as *mut i8;
    CString::from_raw(source);
    let name = std::mem::replace(&mut mapping.name, ptr::null())  as *mut i8;
    CString::from_raw(name);
  }
}

#[no_mangle]
pub extern fn get_error(cache_ptr: *const Cache) -> *const c_char {
  unsafe {
    if cache_ptr.is_null() {
      return ptr::null();
    }
    let cache = &*cache_ptr;

    match &cache.result {
      &Ok(_) => return ptr::null(),
      &Err(ref err) => match CString::new(err.to_owned()) {
        Ok(cstr_err) => cstr_err,
        Err(_) => CString::new("Unknown error").unwrap()
      }
    }.into_raw()
  }
}

#[no_mangle]
pub extern fn error_free(err: *mut c_char) {
  unsafe {
    if err.is_null() { return }
    CString::from_raw(err);
  }
}

fn internal_consume(s: *const c_char) -> Result<jsm::Cache, String> {
  let c_str = unsafe {
    if s.is_null() {
      return Err("Source Map JSON cannot be Null".to_owned());
    }
    CStr::from_ptr(s)
  };

  let r_str = str::from_utf8(c_str.to_bytes()).unwrap();

  jsm::consume(r_str)
}
