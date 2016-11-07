using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Javascript.SourceMapper.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                var json = "{ \"version\": 3, \"file\": \"foo.js\", \"sources\": [\"såurce.js\"], \"names\": [\"näme1\", \"name1\", \"name3\"], \"mappings\": \";EAACA;;IAEEA;;MAEEE\", \"sourceRoot\": \"http://example.com\" }";

                var cache = new SourceMapCache(json);
                var mapping = cache.SourceMappingFor(2, 2);
                Console.WriteLine("OK: {0}", mapping);
            }
            {
                var json = "{ \"version\": 2, \"file\": \"foo.js\", \"sources\": [\"såurce.js\"], \"names\": [\"näme1\", \"name1\", \"name3\"], \"mappings\": \";EAACA;;IAEEA;;MAEEE\", \"sourceRoot\": \"http://example.com\" }";
                try
                {
                    var cache = new SourceMapCache(json);
                    Console.WriteLine("ERR: Should throw on source maps < v3");
                    return;
                }
                catch (SourceMapParsingException e)
                {
                    Console.WriteLine("OK: Does not parse old source maps: {0}", e.Message);
                }
            }
            {
                var json = "{ \"version\": 3, \"file\": \"foo.js\", \"sources\": [\"såurce.js\"], \"names\": [\"näme1\", \"name1\", \"name3\"], \"mappings\": \";;;\", \"sourceRoot\": \"http://example.com\" }";
                try
                {
                    var cache = new SourceMapCache(json);
                    Console.WriteLine("ERR: Should throw on empty source maps");
                    return;
                }
                catch (SourceMapParsingException e)
                {
                    Console.WriteLine("OK: Does not parse empty source maps: {0}", e.Message);
                }
            }
            if(args.Length > 0)
            {
                var jsonPath = args[0];
                Console.WriteLine($"Performance testing with source map: {jsonPath}");
                var watch = new Stopwatch();
                for(var i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Run {i}/10, {watch.ElapsedMilliseconds}ms elapsed");
                    watch.Start();
                    //var json = File.ReadAllText(jsonPath);
                    var jsonStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
                    var cache = new SourceMapCache(jsonStream);
                    var mapping = cache.SourceMappingFor(2, 2);
                    watch.Stop();
                }
                Console.WriteLine($"Average duration (read, parse, query): {watch.ElapsedMilliseconds / 10}ms");
            }
           Console.ReadLine();
        }
    }
}
