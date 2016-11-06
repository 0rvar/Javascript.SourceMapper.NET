using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Javascript.SourceMapper
{
    internal static class Base64Converter
    {
        internal const string CHARACTER_MAP = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static char Encode(int value)
        {
            if(value < 0 || value >= CHARACTER_MAP.Length)
            {
                throw new IndexOutOfRangeException($"Value {value} cannot be encoded as base64");
            }
            return CHARACTER_MAP[value];
        }

        internal static int Decode(char code)
        {
            // 0 - 25: ABCDEFGHIJKLMNOPQRSTUVWXYZ
            if('A' <= code && code <= 'Z')
            {
                return code - 'A';
            }

            // 26 - 51: abcdefghijklmnopqrstuvwxyz
            if('a' <= code && code <= 'z')
            {
                const int lowercase_map_offset = 26;
                return code - 'a' + lowercase_map_offset;
            }

            // 52 - 61: 0123456789
            if('0' <= code && code <= '9')
            {
                const int number_map_offset = 52;
                return code - '0' + number_map_offset;
            }

            // 62: +
            if(code == '+')
            {
                return 62;
            }

            // 63: /
            if(code == '/')
            {
                return 63;
            }

            // Invalid base64 digit.
            throw new IndexOutOfRangeException($"Value '{code}' cannot be decoded as base64");
        }
    }
}
