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
            var json = "{ \"version\": 3, \"file\": \"foo.js\", \"sources\": [\"såurce.js\"], \"names\": [\"näme1\", \"name1\", \"name3\"], \"mappings\": \";EAACA;;IAEEA;;MAEEE\", \"sourceRoot\": \"http://example.com\" }";

            Console.WriteLine("Constructing cache...");
            var cache = new SourceMapCache(json);

            Console.WriteLine("Finding mapping...");
            var mapping = cache.SourceMappingFor(2, 2);

            Console.WriteLine("{0}", mapping);
        }
    }
}
