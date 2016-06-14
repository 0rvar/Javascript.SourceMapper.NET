using System;
using System.Runtime.InteropServices;

namespace Javascript.SourceMapper
{
    internal class RustFFIApi
    {
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern CacheHandle cache_init(string json);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cache_free(IntPtr cache);

        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern string get_error(CacheHandle cache);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void error_free(string error);

        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr find_mapping(CacheHandle cache, UInt32 line, UInt32 column);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mapping_free(IntPtr mapping);
    }

    internal class CacheHandle : SafeHandle
    {
        public CacheHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid
        {
            get { return false; }
        }

        protected override bool ReleaseHandle()
        {
            RustFFIApi.cache_free(handle);
            return true;
        }
    }

    public class SourceMapParsingException : Exception
    {
        public SourceMapParsingException(string message) : base(message) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct FFIMapping
    {
        public uint source_line;
        public uint source_column;
        public uint generated_line;
        public uint generated_column;
        public IntPtr source;
        public IntPtr name;
    }

    public class SourceMapping
    {
        public uint SourceLine;
        public uint SourceColumn;
        public uint GeneratedLine;
        public uint GeneratedColumn;
        public string Source;
        public string Name;
    }

    public class SourceMapCache : IDisposable
    {
        private CacheHandle cache;

        public SourceMapCache(string json)
        {
            cache = RustFFIApi.cache_init(json);
            string temp = RustFFIApi.get_error(cache);
            if(temp != null)
            {
                string error = (string)temp.Clone();
                RustFFIApi.error_free(temp);
                throw new SourceMapParsingException(error);
            }
        }

        public SourceMapping SourceMappingFor(uint line, uint column)
        {
            IntPtr mappingPtr = RustFFIApi.find_mapping(cache, line, column);
            FFIMapping ffiMapping = Marshal.PtrToStructure<FFIMapping>(mappingPtr);
            var mapping = new SourceMapping
            {
                SourceLine = ffiMapping.source_line,
                SourceColumn = ffiMapping.source_column,
                GeneratedLine = ffiMapping.generated_line,
                GeneratedColumn = ffiMapping.generated_column,
                Source = (string)Marshal.PtrToStringUni(ffiMapping.source).Clone(),
                Name = (string)Marshal.PtrToStringUni(ffiMapping.name).Clone()
            };
            RustFFIApi.mapping_free(mappingPtr);
            return mapping;
        }

        public void Dispose()
        {
            cache.Dispose();
        }
    }

}
