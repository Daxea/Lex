using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lex.Demo.WordGenerator
{
    public class WordParser
    {
        public static string Parse(string input, Random random)
        {
            var lexer = new Lexer(input, WordTokens.TokenMappings);
            var parser = new WordParser(lexer, random);
            return parser.Parse();
        }

        private Lexer _lexer;
        private Token _current;
        private Random _rand;

        private readonly string _consonants = "bcdfghjklmnpqrstvwxyz";
        private readonly string _vowels = "aeiouy";

        private WordParser(Lexer lexer, Random rand)
        {
            _lexer = lexer;
            _current = _lexer.Next();
            _rand = rand;
        }

        private string Parse()
        {
            var result = string.Empty;
            var capitalize = false;
            var optional = false;
            while (_current.TypeId != WordTokens.EndInput)
            {
                if (_current.TypeId == WordTokens.CapitalizeNext)
                {
                    capitalize = true;
                    _current = _lexer.Next();
                    continue;
                }

                if (optional && _rand.Next(-50, 50) > 0)
                {
                    capitalize = false;
                    optional = false;
                    _current = _lexer.Next();
                    continue;
                }

                if (_current.TypeId == WordTokens.OptionalNext)
                {
                    optional = true;
                    _current = _lexer.Next();
                    continue;
                }

                if (_current.TypeId == WordTokens.Consonant)
                {
                    var character = _consonants[_rand.Next(0, _consonants.Length)];
                    result += capitalize ? char.ToUpperInvariant(character) : character;
                }

                if (_current.TypeId == WordTokens.Vowel)
                {
                    var character = _vowels[_rand.Next(0, _vowels.Length)];
                    result += capitalize ? char.ToUpperInvariant(character) : character;
                }

                capitalize = false;
                optional = false;
                _current = _lexer.Next();
            }
            return result;
        }
    }
}