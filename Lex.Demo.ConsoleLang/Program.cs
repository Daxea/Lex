using System;
using System.Collections.Generic;
using System.Linq;

using static System.Console;

using Lex.Parsing;

namespace Lex.Demo.ConsoleLang
{
    class Program
    {
        #region Token Type Ids

        public const int Void = 100;
        public const int Null = 101;
        public const int Integer = 102;
        public const int Float = 103;
        public const int Boolean = 104;
        public const int String = 105;
        public const int Identifier = 106;
        public const int True = 107;
        public const int False = 108;

        public const int LParen = 200;
        public const int RParen = 201;
        public const int LBlock = 203;
        public const int RBlock = 204;
        public const int Branch = 220;
        public const int Else = 221;
        public const int Loop = 222;
        public const int Comma = 250;
        public const int Semi = 251;
        public const int Assign = 252;

        public const int TypeOf = 300;
        public const int Func = 301;
        public const int Return = 302;
        public const int Variable = 303;
        public const int Print = 399;

        public const int Plus = 500;
        public const int Minus = 501;
        public const int Multiply = 502;
        public const int Divide = 503;

        #endregion
        #region Token Mappings for ConsoleLang

        private static readonly List<TokenMapping> _mappings = new List<TokenMapping>()
        {
            new TokenMapping(l => char.IsDigit(l.CurrentChar) || (l.CurrentChar == '.' && char.IsDigit(l.Peek())), GetNumber),
            new TokenMapping(l => char.IsLetter(l.CurrentChar) || (l.CurrentChar == '_' && char.IsLetter(l.Peek())), GetIdentifier),
            new TokenMapping(l => l.CurrentChar == '"', GetString), new TokenMapping("+", Plus), new TokenMapping("-", Minus),
            new TokenMapping("(", LParen), new TokenMapping(")", RParen), new TokenMapping("{", LBlock), new TokenMapping("}", RBlock),
            new TokenMapping(",", Comma), new TokenMapping("typeof", TypeOf), new TokenMapping("func", Func), new TokenMapping(";", Semi),
            new TokenMapping("return", Return), new TokenMapping("print", Print), new TokenMapping("if", Branch), new TokenMapping("else", Else),
            new TokenMapping("loop", Loop), new TokenMapping("*", Multiply), new TokenMapping("/", Divide), new TokenMapping("=", Assign),
            new TokenMapping("var", Variable)
        };

        private static Token GetNumber(Lexer lexer)
        {
            Func<string> getNumberString = () =>
            {
                var result = string.Empty;
                while (lexer.CurrentChar != char.MinValue && (char.IsDigit(lexer.CurrentChar) || lexer.CurrentChar == '_'))
                {
                    if (lexer.CurrentChar == '_')
                    {
                        lexer.Advance();
                        continue;
                    }

                    result += lexer.CurrentChar;
                    lexer.Advance();
                }
                return result;
            };

            var resultString = getNumberString();

            if (lexer.CurrentChar == '.')
            {
                resultString += lexer.CurrentChar;
                lexer.Advance();

                resultString += getNumberString();

                if ("fF".Contains(lexer.CurrentChar))
                    lexer.Advance();

                return new Token(Float, float.Parse(resultString));
            }

            if ("fF".Contains(lexer.CurrentChar))
            {
                lexer.Advance();
                return new Token(Float, float.Parse(resultString));
            }

            return new Token(Integer, int.Parse(resultString));
        }

        private static Token GetString(Lexer lexer)
        {
            if (lexer.CurrentChar != '"')
            {
                lexer.Log(new Exception("Malformed string? Strings must begin with a double-quote '\"'"));
            }
            lexer.Advance();
            var result = string.Empty;
            while (lexer.CurrentChar != char.MinValue && lexer.CurrentChar != '"')
            {
                if (lexer.CurrentChar == '\\' && lexer.Peek() == '"')
                {
                    lexer.Advance(2);
                    result += "\"";
                    continue;
                }

                result += lexer.CurrentChar;
                lexer.Advance();
            }
            lexer.Advance();
            return new Token(String, result);
        }

        private static Token GetIdentifier(Lexer lexer)
        {
            var result = string.Empty;
            if (lexer.CurrentChar == '@')
            {
                result += lexer.CurrentChar;
                lexer.Advance();
            }
            while (lexer.CurrentChar != char.MinValue && (char.IsLetterOrDigit(lexer.CurrentChar) || lexer.CurrentChar == '_'))
            {
                result += lexer.CurrentChar;
                lexer.Advance();
            }
            if (result.StartsWith("@"))
                return new Token(Identifier, result);
            return new Token(Identifier, result);
        }

        #endregion

        static void Main(string[] args)
        {
            Title = "Lex Demo [ConsoleLang]";
            SetWindowSize((int)(LargestWindowWidth * 0.75f), (int)(LargestWindowHeight * 0.75f));

            var evaluator = new ExpressionEvaluator();

            var parseTree = new ConsoleLangParser();
            var parser = new Parser(parseTree, _mappings);

            while (true)
            {
                PrintString("@> ", ConsoleColor.Yellow);
                var input = ReadLine();
                evaluator.ClearErrors();
                if (input.Equals("cls"))
                {
                    Clear();
                    continue;
                }
                var node = parser.Parse(input);
                evaluator.Eval(node);
                if (!evaluator.IsSuccess)
                    foreach (var error in evaluator.Errors)
                        PrintString($"{error.Message}\n", ConsoleColor.Red);
            }
        }

        static void PrintString(string text, ConsoleColor color)
        {
            var originalColor = ForegroundColor;
            ForegroundColor = color;
            Write(text);
            ForegroundColor = originalColor;
        }
    }
}