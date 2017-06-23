using System;
using System.Collections.Generic;
using System.Linq;

namespace Lex
{
    /// <summary>
    /// A customizable lexer that can be used to tokenize string inputs.
    /// </summary>
    public class Lexer
    {
        #region Nested Types

        public class LexerConfig
        {
            public List<TokenMapping> TokenMappings { get; set; }
            public string LineCommentStart { get; set; }
            public string BlockCommentStart { get; set; }
            public string BlockCommentEnd { get; set; }

            public List<NumericType> NumericTypes { get; set; }

            public class NumericType
            {
                public string Keyword { get; set; }
                public int TokenTypeId { get; set; }
                public string Suffix { get; set; }
                public Type Type { get; set; }
            }

            public static LexerConfig Default(List<TokenMapping> mappings) => new LexerConfig
            {
                LineCommentStart = "//",
                BlockCommentStart = "/*",
                BlockCommentEnd = "*/",
                TokenMappings = mappings,
                NumericTypes = new List<NumericType>
                {
                    new NumericType
                    {
                        Keyword = "int",
                        TokenTypeId = 50000,
                        Suffix = string.Empty,
                        Type = typeof(int)
                    },
                    new NumericType
                    {
                        Keyword = "float",
                        TokenTypeId = 50001,
                        Suffix = "Ff",
                        Type = typeof(float)
                    },
                    new NumericType
                    {
                        Keyword = "money",
                        TokenTypeId = 50002,
                        Suffix = "$",
                        Type = typeof(decimal)
                    }
                }
            };
        }

        #endregion
        #region Fields/Properties

        private readonly string _text;
        private int _position;
        private char _currentChar;

        public char CurrentChar => _currentChar;

        private readonly LexerConfig _lexerConfig;

        private List<Exception> _errors;
        /// <summary>
        /// Gets an array of any exceptions that may have been logged while tokenizing input.
        /// </summary>
        public Exception[] Errors => _errors.ToArray();

        #endregion
        #region Ctor/Misc

        public Lexer(string text, List<TokenMapping> mappings)
        {
            _text = text;
            _position = 0;
            _currentChar = _text[0];
            _errors = new List<Exception>();
            _lexerConfig = LexerConfig.Default(mappings);
            PrepareTokenMappings();
        }

        public Lexer(string text, LexerConfig lexerConfig)
        {
            _text = text;
            _position = 0;
            _currentChar = _text[0];
            _errors = new List<Exception>();
            _lexerConfig = lexerConfig;
            PrepareTokenMappings();
        }

        private void PrepareTokenMappings()
        {
            _lexerConfig.TokenMappings = _lexerConfig.TokenMappings.OrderByDescending(m => m.Key.Length).ToList();

            _lexerConfig.TokenMappings.InsertRange(0,
                new[] {
                    new TokenMapping(l => MatchTokenKey(_lexerConfig.LineCommentStart), l => { SkipLineComment(); return Token.Empty; }),
                    new TokenMapping(l => MatchTokenKey(_lexerConfig.BlockCommentStart), l => { SkipBlockComment(); return Token.Empty; })
                });
        }

        /// <summary>
        /// Determines whether the current character is the start of the specified key.
        /// </summary>
        /// <param name="key">The key we want to look for at the current character position in the input text.</param>
        /// <returns>Returns true if the key matches, false if no match was found.</returns>
        public bool MatchTokenKey(string key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (!Peek(i, key[i]))
                    return false;
            }
            return true;
        }

        #endregion
        #region Advance Position

        /// <summary>
        /// Advances the lexer position by 1 or a specified number of characters.
        /// </summary>
        /// <param name="num">The number of characters to advance by, or 1 if not specified.</param>
        public void Advance(int num = 1)
        {
            _position += num;
            _currentChar = _position >= _text.Length ? char.MinValue : _text[_position];
        }

        /// <summary>
        /// Returns whether the next character in the input text matches the specified target.
        /// </summary>
        /// <param name="target">The target character to look for.</param>
        /// <returns></returns>
        public bool Peek(char target) => Peek(1, target);

        /// <summary>
        /// Returns whether the character at the specified position ahead of the current position in the input text matches the specified target.
        /// </summary>
        /// <param name="num">The number of characters ahead to peek.</param>
        /// <param name="target">The target character to look for.</param>
        /// <returns></returns>
        public bool Peek(int num, char target) => Peek(num) == target;

        /// <summary>
        /// Returns the next character in the input text.
        /// </summary>
        /// <returns></returns>
        public char Peek() => Peek(1);

        /// <summary>
        /// Returns the character at the specified position ahead of the current character.
        /// </summary>
        /// <param name="num">The number of characters ahead to peek.</param>
        /// <returns></returns>
        public char Peek(int num)
        {
            var peekPos = _position + num;
            if (peekPos >= _text.Length)
                return char.MinValue;
            return _text[peekPos];
        }

        #endregion
        #region Skip Content

        private void SkipWhitespace()
        {
            while (_currentChar != char.MinValue && char.IsWhiteSpace(_currentChar))
                Advance();
        }

        private void SkipLineComment()
        {
            while (!Environment.NewLine.Contains(_currentChar) && (_currentChar != char.MinValue))
                Advance();
            Advance();
        }

        private void SkipBlockComment()
        {
            while (_currentChar != char.MinValue && !MatchTokenKey(_lexerConfig.BlockCommentEnd))
                Advance();
            Advance(_lexerConfig.BlockCommentEnd.Length);
        }

        #endregion
        #region Create Tokens

        /// <summary>
        /// Gets the next token from the input string.
        /// </summary>
        /// <returns>Returns a <see cref="Token"/>.</returns>
        public Token Next()
        {
            while (_currentChar != char.MinValue)
            {
                if (char.IsWhiteSpace(_currentChar))
                {
                    SkipWhitespace();
                    continue;
                }

                Token token = null;
                foreach (var unit in _lexerConfig.TokenMappings)
                {
                    token = unit.Process(this);
                    if (token != null)
                    {
                        if (token.IsEmpty)
                            break;
                        return token;
                    }
                }

                if (token != null && token.IsEmpty)
                    continue;

                Log(new TokenizationFailedException(_currentChar, _position));
                return null;
            }

            return new Token(-1);
        }

        public Token PeekToken()
        {
            if (_currentChar == char.MinValue)
                return Token.Empty;
            var position = _position;
            var token = Next();
            _position = position;
            _currentChar = _text[_position];
            return token;
        }

        public Token[] GetAllTokens()
        {
            var tokens = new List<Token>();
            var token = Next();
            while (token != null)
            {
                tokens.Add(token);
                token = Next();
            }
            return tokens.ToArray();
        }

        public Token GetString(int stringTypeId)
        {
            // Skip the initial string character
            Advance();
            var result = string.Empty;
            while (CurrentChar != char.MinValue && CurrentChar != '"')
            {
                if (CurrentChar == '\\' && Peek() == '"')
                {
                    Advance(2);
                    result += "\"";
                    continue;
                }

                result += CurrentChar;
                Advance();
            }
            Advance();
            return new Token(stringTypeId, result);
        }

        public Token GetIdentifier(int identifierTypeId)
        {
            var result = string.Empty;
            if (CurrentChar == '@')
            {
                result += CurrentChar;
                Advance();
            }
            while (CurrentChar != char.MinValue && (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_'))
            {
                result += CurrentChar;
                Advance();
            }
            return new Token(identifierTypeId, result);
        }

        public Token GetNumber(int integerTokenId, int floatTokenId)
        {
            Func<string> getNumberString = () =>
            {
                var result = string.Empty;
                while (CurrentChar != char.MinValue && (char.IsDigit(CurrentChar) || CurrentChar == '_'))
                {
                    if (CurrentChar == '_')
                    {
                        Advance();
                        continue;
                    }

                    result += CurrentChar;
                    Advance();
                }
                return result;
            };

            var resultString = getNumberString();

            if (CurrentChar == '.')
            {
                resultString += CurrentChar;
                Advance();

                resultString += getNumberString();

                if ("Ff".Contains(CurrentChar))
                    Advance();

                return new Token(floatTokenId, float.Parse(resultString));
            }

            if ("Ff".Contains(CurrentChar))
            {
                Advance();
                return new Token(floatTokenId, float.Parse(resultString));
            }

            return new Token(integerTokenId, int.Parse(resultString));
        }

        #endregion
        #region Logs

        public void Log(Exception error) => _errors.Add(error);

        #endregion
    }
}