using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Javascript.SourceMapper
{
    internal class RustFfiApi32
    {
        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern CacheHandle32 cache_init([In] byte[] json);
        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cache_free(IntPtr cache);

        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_error(CacheHandle32 cache);
        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void error_free(IntPtr error);

        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr find_mapping(CacheHandle32 cache, UInt32 line, UInt32 column);
        [DllImport("JsSourceMapper_FFI_32", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mapping_free(IntPtr mapping);
    }

    internal class CacheHandle32 : SafeHandle
    {
        public CacheHandle32() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            RustFfiApi32.cache_free(handle);
            return true;
        }
    }

    internal class RustFfiApi64
    {
        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern CacheHandle64 cache_init([In] byte[] json);
        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cache_free(IntPtr cache);

        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_error(CacheHandle64 cache);
        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void error_free(IntPtr error);

        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr find_mapping(CacheHandle64 cache, UInt32 line, UInt32 column);
        [DllImport("JsSourceMapper_FFI_64", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mapping_free(IntPtr mapping);
    }

    internal class CacheHandle64 : SafeHandle
    {
        public CacheHandle64() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            RustFfiApi64.cache_free(handle);
            return true;
        }
    }

    internal class Utils
    {
        public static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static byte[] NullTerminatedUtf8Bytes(string str)
        {
            return Encoding.UTF8.GetBytes(str + "\0");
        }
    }

    /// <summary>
    /// Raised for mailformed of empty source maps, contains Message explaining why parsing failed.
    /// </summary>
    public class SourceMapParsingException : Exception
    {
        /// <summary>
        /// Constructs a new SourceMapParsingException with a specific message
        /// </summary>
        /// <param name="message">Error message</param>
        public SourceMapParsingException(string message) : base(message) { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct FfiMapping
    {
        public uint source_line;
        public uint source_column;
        public uint generated_line;
        public uint generated_column;
        public IntPtr source;
        public IntPtr name;
    }

    /// <summary>
    /// Represents a mapping from a generated position to a source position and, optionally, source file name and function name.
    /// </summary>
    public class SourceMapping
    {
        public uint SourceLine;
        public uint SourceColumn;
        public uint GeneratedLine;
        public uint GeneratedColumn;
        public string Source;
        public string Name;

        public override string ToString()
        {
            return string.Format("SourceMapping[{0}::{1}]{{ {2},{3} => {4},{5} }}", 
                Source, Name, GeneratedLine, GeneratedColumn, SourceLine, SourceColumn
            );
        }
    }

    /// <summary>
    /// Represents a processed source map that can be queries for generated->source mapping information.
    /// </summary>
    public class SourceMapCache : IDisposable
    {
        private readonly CacheHandle32 _cache32;
        private readonly CacheHandle64 _cache64;

        /// <summary>
        /// Processes a source map and constructs a cache for fast mapping lookups.
        /// </summary>
        /// <param name="json">Source Map in JSON format</param>
        /// <exception cref="SourceMapParsingException">If the source map is malformed</exception>
        public SourceMapCache(string json)
        {
            if (Is64Bit())
            {
                _cache64 = RustFfiApi64.cache_init(Utils.NullTerminatedUtf8Bytes(json));
            }
            else
            {
                _cache32 = RustFfiApi32.cache_init(Utils.NullTerminatedUtf8Bytes(json));
            }
            string error = getError();
            if (error != null)
            {
                throw new SourceMapParsingException(error);
            }
        }

        private string getError() {
            IntPtr errPtr;

            if(Is64Bit())
            {
                errPtr = RustFfiApi64.get_error(_cache64);
            } else
            {
                errPtr = RustFfiApi32.get_error(_cache32);
            }

            if(errPtr == IntPtr.Zero)
            {
                return null;
            }

            string error = Utils.StringFromNativeUtf8(errPtr);

            if (Is64Bit())
            {
                RustFfiApi64.error_free(errPtr);
            }
            else
            {
                RustFfiApi32.error_free(errPtr);
            }

            return error;
        }

        /// <summary>
        /// Finds the mapping for a generated position to the source position,
        /// and, if provided by the source map, source file name and source function names.
        /// </summary>
        /// <param name="line">Line in the generated JavaScript file</param>
        /// <param name="column">Column in the generated JavaScript file</param>
        /// <returns></returns>
        public SourceMapping SourceMappingFor(uint line, uint column)
        {
            IntPtr mappingPtr;

            if (Is64Bit())
            {
                mappingPtr = RustFfiApi64.find_mapping(_cache64, line, column);
            }
            else
            {
                mappingPtr = RustFfiApi32.find_mapping(_cache32, line, column);
            }

            FfiMapping ffiMapping = Marshal.PtrToStructure<FfiMapping>(mappingPtr);
            var mapping = new SourceMapping
            {
                SourceLine = ffiMapping.source_line,
                SourceColumn = ffiMapping.source_column,
                GeneratedLine = ffiMapping.generated_line,
                GeneratedColumn = ffiMapping.generated_column,
                Source = Utils.StringFromNativeUtf8(ffiMapping.source),
                Name = Utils.StringFromNativeUtf8(ffiMapping.name)
            };

            if (Is64Bit())
            {
                RustFfiApi64.mapping_free(mappingPtr);
            }
            else
            {
                RustFfiApi32.mapping_free(mappingPtr);
            }
            return mapping;
        }

        public void Dispose()
        {
            if (_cache64 != null)
            {
                _cache64.Dispose();
            }
            if (_cache32 != null)
            {
                _cache32.Dispose();
            }
        }

        private bool Is64Bit() {
            return IntPtr.Size == 8;
        }
    }

}
