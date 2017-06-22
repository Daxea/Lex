using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lex.Parsing
{
    public sealed class Parser
    {
        public class ParserInstance
        {
            private Lexer _lexer;
            private Token _current;
            private Token _previous;

            public Token Current => _current;

            public ParserInstance(Lexer lexer)
            {
                _lexer = lexer;
                _current = _lexer.Next();
                _previous = null;
            }

            public void ConsumeToken() => ConsumeToken(_current.TypeId);

            public void ConsumeToken(int tokenTypeId)
            {
                if (_current.TypeId != tokenTypeId)
                    throw new Exception($"Token Mismatch! Expected: {tokenTypeId} | Actual: {_current.TypeId}");
                _previous = _current;
                _current = _lexer.Next();
            }

            public Token Peek() => _lexer.PeekToken();
        }

        private readonly ParseNode _parseTree;
        private readonly List<TokenMapping> _tokenMappings;

        public Parser(ParseNode parseTree, List<TokenMapping> tokenMappings)
        {
            _parseTree = parseTree;
            _tokenMappings = tokenMappings;
        }

        public SyntaxNode Parse(string input)
        {
            var lexer = new Lexer(input, _tokenMappings);
            return _parseTree.Parse(new ParserInstance(lexer));
        }
    }

    public abstract class ParseNode
    {
        public abstract SyntaxNode Parse(Parser.ParserInstance parser);
    }

    public abstract class SyntaxNode { }
}