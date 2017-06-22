using System;

namespace Lex
{
    public class TokenizationFailedException : Exception
    {
        public TokenizationFailedException(char character, int position) : base($"Tokenization failed at character '{character}', position: '{position}'") { }
    }
}