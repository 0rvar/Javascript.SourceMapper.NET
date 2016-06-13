using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Javascript.SourceMapper.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = "{ \"version\": 3, \"file\": \"foo.js\", \"sources\": [\"source.js\"], \"names\": [\"name1\", \"name1\", \"name3\"], \"mappings\": \";EAACA;;IAEEA;;MAEEE\", \"sourceRoot\": \"http://example.com\" }";
            var cache = new SourceMapCache(json);

            var mapping = cache.SourceMappingFor(2, 2);

            Console.WriteLine("{0}", mapping);
        }
    }
}
