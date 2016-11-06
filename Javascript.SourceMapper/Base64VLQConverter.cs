using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Javascript.SourceMapper
{
    internal static class Base64VLQConverter
    {
        const int VLQ_BASE_SHIFT = 5;
        const int VLQ_BASE = 1 << VLQ_BASE_SHIFT;
        const int VLQ_BASE_MASK = VLQ_BASE - 1;
        const int VLQ_CONTINUATION_BIT = VLQ_BASE;

        public static int toVLQ(int value)
        {
            if(value < 0)
            {
                return ((-value) << 1) + 1;
            }
            else
            {
                return (value << 1) + 0;
            }
        }

        public static int fromVLQ(int value)
        {
            var isNegative = (value & 1) == 1;
            var shifted = value >> 1;
            if(isNegative) {
                return -shifted;
            }
            else
            {
                return shifted;
            }
        }
        /// <summary>
        /// Encodes a number in the Base64 VLQ format
        /// </summary>
        /// <param name="value">The value to be encoded</param>
        /// <returns>The value encoded as a Base64 VLQ string</returns>
        public static string Encode(int value)
        {
            var encoded = new List<char>();
            var vlq = toVLQ(value);
            do
            {
                var digit = vlq & VLQ_BASE_MASK;
                vlq = vlq >> VLQ_BASE_SHIFT;
                if (vlq > 0)
                {
                    digit |= VLQ_CONTINUATION_BIT;
                }

                encoded.Add(Base64Converter.Encode(digit));
            }
            while (vlq > 0);
            return string.Concat(encoded);
        }

        public static DecodeResult Decode(string encoded)
        {
            int result = 0;
            int shift = 0;

            int charactersRead = 0;
            foreach(var character in encoded)
            {
                charactersRead += 1;

                var digit = Base64Converter.Decode(character);
                var continuation = (digit & VLQ_CONTINUATION_BIT) != 0;
                digit &= VLQ_BASE_MASK;
                result += (digit << shift);
                shift += VLQ_BASE_SHIFT;
                if(!continuation) {
                    break;
                }
            }
            return new DecodeResult(fromVLQ(result), charactersRead);
        }

        public class DecodeResult
        {
            public int CharactersRead;
            public int Result;

            public DecodeResult(int result, int charactersRead)
            {
                this.Result = result;
                this.CharactersRead = charactersRead;
            }
        }
    }
}
