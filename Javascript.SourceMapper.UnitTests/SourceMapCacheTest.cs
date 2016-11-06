using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Javascript.SourceMapper.UnitTests
{
    [TestClass]
    public class SourceMapCacheTest
    {
        [TestMethod]
        public void SourceMapCache_ParsesSimpleSourceMap()
        {
            var cache = new SourceMapCache(new SourceMap()
            {
                version = 3,
                file = "foo.js",
                sourceRoot = "http://example.com/",
                sources = new string[] { "/a" },
                names = new string[] { },
                mappings = "AACA"
            });

            var expected = new SourceMapping(2, 0, 1, 0, "/a", "");
            var actual = cache.SourceMappingFor(1, 0);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SourceMapCache_HandlesDuplicateSources()
        {
            var cache = new SourceMapCache(new SourceMap()
            {
                version = 3,
                file = "foo.js",
                sources = new string[] { "source1.js", "source1.js", "source3.js" },
                names = new string[] { },
                mappings = ";EAAC;;IAEE;;MEEE",
                sourceRoot = "http://example.com"
            });


            {
                var expected = new SourceMapping(1, 1, 2, 2, "source1.js", "");
                var actual = cache.SourceMappingFor(2, 2);
                Assert.AreEqual(expected, actual);
            }

            {
                var expected = new SourceMapping(3, 3, 4, 4, "source1.js", "");
                var actual = cache.SourceMappingFor(4, 4);
                Assert.AreEqual(expected, actual);
            }

            {
                var expected = new SourceMapping(5, 5, 6, 6, "source3.js", "");
                var actual = cache.SourceMappingFor(6, 6);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void SourceMapCache_HandlesDuplicateNames()
        {
            var cache = new SourceMapCache(new SourceMap()
            {
                version = 3,
                file = "foo.js",
                sources = new string[] { "source.js" },
                names = new string[] { "name1", "name1", "name3" },
                mappings = ";EAACA;;IAEEA;;MAEEE",
                sourceRoot = "http://example.com"
            });

            {
                var expected = new SourceMapping(1, 1, 2, 2, "source.js", "name1");
                var actual = cache.SourceMappingFor(2, 2);
                Assert.AreEqual(expected, actual);
            }

            {
                var expected = new SourceMapping(3, 3, 4, 4, "source.js", "name1");
                var actual = cache.SourceMappingFor(4, 4);
                Assert.AreEqual(expected, actual);
            }

            {
                var expected = new SourceMapping(5, 5, 6, 6, "source.js", "name3");
                var actual = cache.SourceMappingFor(6, 6);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void SourceMapCache_AllowsOmittingSourceRoot()
        {
            var cache = new SourceMapCache(new SourceMap()
            {
                version = 3,
                file = "foo.js",
                sources = new string[] { "source.js" },
                names = new string[] { "name1", "name1", "name3" },
                mappings = ";EAACA;;IAEEA;;MAEEE"
            });
            // Should not throw
        }

        [TestMethod]
        public void SourceMapCache_RejectsOlderSourceMapRevisions()
        {
            try
            {
                var cache = new SourceMapCache(new SourceMap()
                {
                    version = 2,
                    file = "",
                    sources = new string[] { "source.js" },
                    names = new string[] { "name1", "name1", "name3" },
                    mappings = ";EAACA;;IAEEA;;MAEEE",
                    sourceRoot = "http://example.com"
                });
                Assert.Fail("Source Map revision < 3 should be rejected");
            }
            catch (SourceMapParsingException e)
            {
                // OK
            }
        }

        [TestMethod]
        public void SourceMapCache_RejectsInvalidSourceMapsNormally()
        {
            try
            {
                var cache = new SourceMapCache(new SourceMap()
                {
                    version = 3,
                    file = "",
                    sources = new string[] { },
                    names = new string[] { },
                    mappings = ";EAACA;;IAEEA;;MAEEE"
                });
                Assert.Fail("Invalid source maps should be rejected");
            }
            catch (SourceMapParsingException e)
            {
                // OK
            }
        }

        [TestMethod]
        public void SourceMapCache_ThrowsWhenThereAreNoMappings()
        {
            try
            {
                var cache = new SourceMapCache(new SourceMap()
                {
                    version = 3,
                    file = "foo.js",
                    sources = new string[] { "source.js" },
                    names = new string[] { "name1", "name1", "name3" },
                    mappings = ";;;"
                });
                Assert.Fail("Source maps with no mappings should be rejected");
            }
            catch (SourceMapParsingException e)
            {
                // OK
            }
        }

        [TestMethod]
        public void SourceMapCache_ParsesEmptySourceRoot()
        {
            var cache = new SourceMapCache(new SourceMap()
            {
                version = 3,
                file = "foo.js",
                sources = new string[] { "source.js" },
                names = new string[] { "name1", "name1", "name3" },
                mappings = ";EAACA;;IAEEA;;MAEEE",
                sourceRoot = ""
            });
        }

    }
}
