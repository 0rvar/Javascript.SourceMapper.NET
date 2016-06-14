using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Javascript.SourceMapper
{
    internal class RustFfiApi
    {
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern CacheHandle cache_init([In] byte[] json);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void cache_free(IntPtr cache);

        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr get_error(CacheHandle cache);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void error_free(IntPtr error);

        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr find_mapping(CacheHandle cache, UInt32 line, UInt32 column);
        [DllImport("JsSourceMapper_FFI", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mapping_free(IntPtr mapping);
    }

    internal class CacheHandle : SafeHandle
    {
        public CacheHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            RustFfiApi.cache_free(handle);
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
        private readonly CacheHandle _cache;

        /// <summary>
        /// Processes a source map and constructs a cache for fast mapping lookups.
        /// </summary>
        /// <param name="json">Source Map in JSON format</param>
        /// <exception cref="SourceMapParsingException">If the source map is malformed</exception>
        public SourceMapCache(string json)
        {
            _cache = RustFfiApi.cache_init(Utils.NullTerminatedUtf8Bytes(json));
            IntPtr errPtr = RustFfiApi.get_error(_cache);
            if(errPtr != IntPtr.Zero)
            {
                string error = Utils.StringFromNativeUtf8(errPtr);
                RustFfiApi.error_free(errPtr);
                throw new SourceMapParsingException(error);
            }
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
            IntPtr mappingPtr = RustFfiApi.find_mapping(_cache, line, column);
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
            RustFfiApi.mapping_free(mappingPtr);
            return mapping;
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }

}
