using System;

namespace Lex
{
    /// <summary>
    /// Represents a mapping between some input in the lexer and the token that should be created for that input.
    /// 
    /// For example, a '+' in the lexer should result in an Add token, while the keyword 'if' should result in a Branch token.
    /// </summary>
    public class TokenMapping
    {
        /// <summary>
        /// The key is the string value of this mapping in the lexer. This is the "+" of the Add token, or the "if" of the Branch token.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// This is the predicate used to test if the lexer is currently consuming the Key, and therefore should create the token for this mapping.
        /// </summary>
        private Func<Lexer, bool> _predicate;
        /// <summary>
        /// This is the function used to create the token if the lexer is currently consuming the Key.
        /// </summary>
        private Func<Lexer, Token> _createToken;

        public TokenMapping(string key, int tokenTypeId)
        {
            Key = key;
            _predicate = DefaultPredicate;
            _createToken = lexer => new Token(tokenTypeId);
        }

        public TokenMapping(Func<Lexer, bool> predicate, int tokenTypeId)
        {
            Key = string.Empty;
            _predicate = predicate;
            _createToken = lexer => new Token(tokenTypeId);
        }

        public TokenMapping(string key, Func<Lexer, Token> createToken)
        {
            Key = key;
            _predicate = DefaultPredicate;
            _createToken = createToken;
        }

        public TokenMapping(Func<Lexer, bool> predicate, Func<Lexer, Token> createToken)
        {
            Key = string.Empty;
            _predicate = predicate;
            _createToken = createToken;
        }

        /// <summary>
        /// Returns the token for this mapping if the <paramref name="lexer"/> is currently consuming the Key (or the <see cref="_predicate"/> returns true).
        /// Returns null if the <see cref="_predicate"/> returns false.
        /// </summary>
        /// <param name="lexer"></param>
        /// <returns>Returns a <see cref="Token"/> or null.</returns>
        internal Token Process(Lexer lexer)
        {
            if (!_predicate(lexer))
                return null;

            var token = _createToken(lexer);
            if (!Key.Equals(string.Empty))
                lexer.Advance(Key.Length);
            return token;
        }

        /// <summary>
        /// This is the default predicate used when a Key is specified.
        /// </summary>
        private Func<Lexer, bool> DefaultPredicate => lexer => lexer.MatchTokenKey(Key);
    }
}