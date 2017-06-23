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
            new TokenMapping(l => char.IsDigit(l.CurrentChar) || (l.CurrentChar == '.' && char.IsDigit(l.Peek())), l => l.GetNumber(Integer, Float)),
            new TokenMapping(l => char.IsLetter(l.CurrentChar) || (l.CurrentChar == '_' && char.IsLetter(l.Peek())), l => l.GetIdentifier(Identifier)),
            new TokenMapping("\"", l => l.GetString(String)), new TokenMapping("+", Plus), new TokenMapping("-", Minus),
            new TokenMapping("(", LParen), new TokenMapping(")", RParen), new TokenMapping("{", LBlock), new TokenMapping("}", RBlock),
            new TokenMapping(",", Comma), new TokenMapping("typeof", TypeOf), new TokenMapping("func", Func), new TokenMapping(";", Semi),
            new TokenMapping("return", Return), new TokenMapping("print", Print), new TokenMapping("if", Branch), new TokenMapping("else", Else),
            new TokenMapping("loop", Loop), new TokenMapping("*", Multiply), new TokenMapping("/", Divide), new TokenMapping("=", Assign),
            new TokenMapping("var", Variable)
        };

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