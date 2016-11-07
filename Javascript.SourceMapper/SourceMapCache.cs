using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Javascript.SourceMapper
{
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

    /// <summary>
    /// Represents a mapping from a generated position to a source position and, optionally, source file name and function name.
    /// </summary>
    public class SourceMapping
    {
        public uint SourceLine { get; internal set;  }
        public uint SourceColumn { get; internal set; }
        public uint GeneratedLine { get; internal set; }
        public uint GeneratedColumn { get; internal set; }
        public string SourceFile { get; internal set; }
        public string SourceName { get; internal set; }

        internal SourceMapping() {}
        internal SourceMapping(uint sourceLine, uint sourceColumn, uint generatedLine, uint generatedColumn, string sourceFile, string sourceName)
        {
            SourceLine = sourceLine;
            SourceColumn = sourceColumn;
            GeneratedLine = generatedLine;
            GeneratedColumn = generatedColumn;
            SourceFile = sourceFile;
            SourceName = sourceName;
        }

        public override bool Equals(object obj)
        {
            var item = obj as SourceMapping;

            if (item == null)
            {
                return false;
            }

            return (
                SourceLine == item.SourceLine &&
                SourceColumn == item.SourceColumn &&
                GeneratedLine == item.GeneratedLine &&
                GeneratedColumn == item.GeneratedColumn &&
                SourceFile == item.SourceFile &&
                SourceName == item.SourceName
            );
        }

        public override string ToString()
        {
            return string.Format("SourceMapping[{0}::{1}]{{ {2},{3} => {4},{5} }}",
                SourceFile, SourceName, GeneratedLine, GeneratedColumn, SourceLine, SourceColumn
            );
        }
    }

    /// <summary>
    /// Represents a processed source map that can be queries for generated->source mapping information.
    /// The only parameter is the raw source map as a JSON string.
    /// According to the [source map spec][source-map-spec], source maps have the following attributes:
    ///
    ///   - version: Which version of the source map spec this map is following.
    ///   - sources: An array of URLs to the original source files.
    ///   - names: An array of identifiers which can be referrenced by individual mappings.
    ///   - sourceRoot: Optional. The URL root from which all sources are relative.
    ///   - sourcesContent: Optional. An array of contents of the original source files.
    ///   - mappings: A string of base64 VLQs which contain the actual mappings.
    ///   - file: Optional. The generated file this source map is associated with.
    ///
    /// Here is an example source map:
    ///
    /// ```json
    ///     {
    ///       "version": 3,
    ///       "file": "out.js",
    ///       "sourceRoot" : "",
    ///       "sources": ["foo.js", "bar.js"],
    ///       "names": ["src", "maps", "are", "fun"],
    ///       "mappings": "AA,AB;;ABCDE;"
    ///     }
    /// ```
    ///
    /// [source-map-spec]: https://docs.google.com/document/d/1U1RGAehQwRypUTovF1KRlpiOFze0b-_2gc6fAH0KY0k/edit?pli=1#
    /// </summary>
    public class SourceMapCache
    {
        private const uint SOURCE_MAP_VERSION = 3;

        private readonly SourceMap sourceMap;
        private List<SourceMapping> generatedMappings;

        /// <summary>
        /// Processes a source map and constructs a cache for fast mapping lookups.
        /// </summary>
        /// <param name="json">Source Map in JSON format as a string</param>
        /// <exception cref="SourceMapParsingException">If the source map is malformed</exception>
        public SourceMapCache(string json) : this(new MemoryStream(Encoding.UTF8.GetBytes(json))) { }

        /// <summary>
        /// Processes a source map and constructs a cache for fast mapping lookups.
        /// </summary>
        /// <param name="json">Source Map in JSON format as a stream</param>
        /// <exception cref="SourceMapParsingException">If the source map is malformed</exception>
        public SourceMapCache(Stream json)
        {
            var serializer = new DataContractJsonSerializer(typeof(SourceMap));
            sourceMap = (SourceMap)serializer.ReadObject(json);
            processSourceMap();
        }

        internal SourceMapCache(SourceMap sourceMap)
        {
            this.sourceMap = sourceMap;
            processSourceMap();
        }

        void processSourceMap()
        {
            if(sourceMap.version != SOURCE_MAP_VERSION)
            {
                throw new SourceMapParsingException("Invalid source map: version != 3");
            }

            var numSources = sourceMap.sources.Length;
            var numNames = sourceMap.names.Length;

            var generatedMappings = new List<SourceMapping>();

            uint generatedLine = 0;
            uint previousOriginalLine = 0;
            uint previousOriginalColumn = 0;
            uint previousSource = 0;
            uint previousName = 0;

            foreach(string line in sourceMap.mappings.Split(';'))
            {
                generatedLine += 1;
                uint previousGeneratedColumn = 0;

                foreach(string segment in line.Split(','))
                {
                    var segmentLength = segment.Length;
                    var fields = new List<int>();
                    var characterIndex = 0;
                    while(characterIndex < segmentLength)
                    {
                        var field = Base64VLQConverter.Decode(segment.Substring(characterIndex, segmentLength - characterIndex));
                        fields.Add(field.Result);
                        characterIndex += field.CharactersRead;
                    }

                    var numFields = fields.Count;
                    if(numFields < 1)
                    {
                        continue;
                    }

                    if(numFields == 2)
                    {
                        throw new SourceMapParsingException("Found a source, but no line and column");
                    }

                    if (numFields == 3)
                    {
                        throw new SourceMapParsingException("Found a source and line, but no column");
                    }

                    previousGeneratedColumn = (uint)(previousGeneratedColumn + fields[0]);

                    var mapping = new SourceMapping()
                    {
                        GeneratedLine = generatedLine,
                        GeneratedColumn = previousGeneratedColumn
                    };

                    if(numFields < 2)
                    {
                        mapping.SourceFile = "";
                        mapping.SourceName = "";
                    }
                    else
                    {
                        previousSource = (uint)(previousSource + fields[1]);
                        if(previousSource >= 0 && previousSource < numSources)
                        {
                            mapping.SourceFile = sourceMap.sources[previousSource];
                        }
                        else
                        {
                            throw new SourceMapParsingException($"Invalid source map: reference to source index {previousSource} when source list length is {numSources}");
                        }

                        previousOriginalLine = (uint)(previousOriginalLine + fields[2]);
                        mapping.SourceLine = previousOriginalLine + 1;

                        previousOriginalColumn = (uint)(previousOriginalColumn + fields[3]);
                        mapping.SourceColumn = previousOriginalColumn;

                        if(numFields > 4)
                        {
                            previousName = (uint)(previousName + fields[4]);
                            if (previousName >= 0 && previousName < numNames)
                            {
                                mapping.SourceName = sourceMap.names[previousName];
                            }
                            else
                            {
                                throw new SourceMapParsingException($"Invalid source map: reference to name index {previousName} when name list length is {numNames}");
                            }
                        }
                        else
                        {
                            mapping.SourceName = "";
                        }
                    }

                    generatedMappings.Add(mapping);
                }
            }

            
            if(generatedMappings.Count < 1)
            {
                throw new SourceMapParsingException("Source map contains no mappings");
            }

            generatedMappings.Sort(new CompareGeneratedItems());

            this.generatedMappings = generatedMappings;
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
            var mockItem = new SourceMapping()
            {
                GeneratedLine = line,
                GeneratedColumn = column
            };
            var position = generatedMappings.BinarySearch(mockItem, new CompareGeneratedItems());
            if(position < 0)
            {
                return generatedMappings[Math.Min(~position, generatedMappings.Count - 1)];
            }
            else
            {
                return generatedMappings[position];
            }
        }

        private class CompareGeneratedItems : IComparer<SourceMapping>
        {
            public int Compare(SourceMapping x, SourceMapping y)
            {
                int result = x.GeneratedLine.CompareTo(y.GeneratedLine);
                return result == 0 ? x.GeneratedColumn.CompareTo(y.GeneratedColumn) : result;
            }
        }
    }

    internal class CodePosition
    {
        public readonly uint line;
        public readonly uint column;
    }

    [DataContract]
    internal class SourceMap
    {
        [DataMember]
        public uint version;
        [DataMember]
        public string[] sources;
        [DataMember]
        public string[] names;
        [DataMember]
        public string sourceRoot;
        [DataMember]
        public string mappings;
        [DataMember]
        public string file;
    }
}
