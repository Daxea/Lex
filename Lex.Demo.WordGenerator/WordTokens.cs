using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lex.Demo.WordGenerator
{
    public static class WordTokens
    {
        // c
        public const int Consonant = 100;
        // v
        public const int Vowel = 101;

        // !
        public const int CapitalizeNext = 200;
        // ?
        public const int OptionalNext = 201;

        // Anytime Lex hits the end of the input, it returns a token with a TypeId of -1,
        // but I like to have a constant for that token type id.
        public const int EndInput = -1;

        public static List<TokenMapping> TokenMappings => new List<TokenMapping>
        {
            new TokenMapping("c", Consonant), new TokenMapping("v", Vowel),
            new TokenMapping("!", CapitalizeNext), new TokenMapping("?", OptionalNext)
        };
    }
}