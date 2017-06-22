using System;
using System.Collections.Generic;
using System.Linq;

using Lex.Parsing;

namespace Lex.Demo.ConsoleLang
{
    public class ConsoleLangParser : ParseNode
    {
        public override SyntaxNode Parse(Parser.ParserInstance parser) => Statement(parser);

        private BlockNode Block(Parser.ParserInstance parser)
        {
            parser.ConsumeToken(Program.LBlock);
            var nodes = Statements(parser);
            parser.ConsumeToken(Program.RBlock);
            return new BlockNode(nodes);
        }

        private SyntaxNode[] Statements(Parser.ParserInstance parser)
        {
            var statements = new List<SyntaxNode> { Statement(parser) };

            while (parser.Current.TypeId == Program.Semi)
            {
                parser.ConsumeToken(Program.Semi);
                statements.Add(Statement(parser));
            }

            return statements.ToArray();
        }

        private SyntaxNode Statement(Parser.ParserInstance parser)
        {
            if (parser.Current.TypeId == Program.LBlock) return Block(parser);
            else if (parser.Current.TypeId == Program.Variable)
            {
                parser.ConsumeToken();
                return VariableDeclaration(parser, TypeNode.Inferred);
            }
            else if (parser.Current.TypeId == Program.Identifier)
            {
                var ident = (string)parser.Current.Value;
                parser.ConsumeToken(Program.Identifier);
                if (parser.Current.TypeId == Program.LParen) // FUNC-INVOKE
                    return FunctionInvoke(parser, ident);
                else if (parser.Current.TypeId == Program.Assign) // ASSIGN
                    return Assignment(parser, new VariableNode(ident));
                else if (parser.Current.TypeId == Program.Identifier)
                    return VariableDeclaration(parser, new TypeNode(ident));
                // increment-by, decrement-by, multiply-by, divide-by
            }
            else if (parser.Current.TypeId == Program.Func) // FUNC-DEFINE
            {
                parser.ConsumeToken(); // consume the FUNC
                var returnType = new TypeNode((string)parser.Current.Value);
                parser.ConsumeToken(); // Consume the Return Type
                var ident = (string)parser.Current.Value;
                parser.ConsumeToken(Program.Identifier);
                parser.ConsumeToken(Program.LParen);
                var parameters = new List<FunctionParameterDefinitionNode>();
                while (parser.Current.TypeId != Program.RParen)
                {
                    if (parser.Current.TypeId == Program.Comma)
                        parser.ConsumeToken();
                    var paramType = new TypeNode((string)parser.Current.Value);
                    parser.ConsumeToken();
                    var paramName = (string)parser.Current.Value;
                    parser.ConsumeToken();
                    parameters.Add(new FunctionParameterDefinitionNode(paramName, paramType));
                }
                parser.ConsumeToken(Program.RParen);
                BlockNode body = null;
                if (parser.Current.TypeId == Program.LBlock)
                    body = Block(parser);
                else if (returnType.Name != "void" && parser.Current.TypeId != Program.Return)
                    body = new BlockNode(new ReturnNode(Expression(parser)));
                else
                    body = new BlockNode(Statement(parser));
                return new FunctionDefinitionNode(ident, returnType, parameters.ToArray(), body);
            }
            else if (parser.Current.TypeId == Program.TypeOf)
            {
                parser.ConsumeToken();
                parser.ConsumeToken(Program.LParen);
                var expression = Expression(parser);
                parser.ConsumeToken(Program.RParen);
                return new TypeOfNode(expression);
            }
            else if (parser.Current.TypeId == Program.Return)
            {
                parser.ConsumeToken();
                return new ReturnNode(Expression(parser));
            }
            else if (parser.Current.TypeId == Program.Branch)
            {
                parser.ConsumeToken();
                return Branch(parser);
            }
            else if (parser.Current.TypeId == Program.Loop)
            {
                parser.ConsumeToken();
                var numberOfLoops = Expression(parser);
                if (parser.Current.TypeId == Program.LBlock)
                    return new LoopNode(numberOfLoops, Block(parser));
                else
                    return new LoopNode(numberOfLoops, Statement(parser));
            }
            else if (parser.Current.TypeId == Program.Print)
            {
                parser.ConsumeToken();
                return new PrintNode(Expression(parser));
            }
            return new NoOpNode();
        }

        private SyntaxNode VariableDeclaration(Parser.ParserInstance parser, TypeNode type)
        {
            var variables = Variables(parser);
            if (type.IsInferred && parser.Current.TypeId != Program.Assign)
                throw new Exception($"Cannot infer the type of implicit variables {string.Join(",", variables.Select(x => x.Name).ToArray())}");
            if (parser.Current.TypeId == Program.Assign)
            {
                parser.ConsumeToken();
                return new VariableDeclarationNode(type, Expression(parser), variables);
            }
            return new VariableDeclarationNode(type, variables);
        }

        private VariableNode[] Variables(Parser.ParserInstance parser)
        {
            var variables = new List<VariableNode> { Variable(parser) };
            while (parser.Current.TypeId == Program.Comma)
            {
                parser.ConsumeToken();
                variables.Add(Variable(parser));
            }
            return variables.ToArray();
        }

        private VariableNode Variable(Parser.ParserInstance parser)
        {
            var node = new VariableNode((string)parser.Current.Value);
            parser.ConsumeToken(Program.Identifier);
            return node;
        }

        private SyntaxNode Assignment(Parser.ParserInstance parser, VariableNode variable)
        {
            parser.ConsumeToken();
            var right = Expression(parser);
            return new AssignNode(variable, right);
        }

        private SyntaxNode Expression(Parser.ParserInstance parser)
        {
            if (parser.Current.TypeId == Program.Identifier)
            {
                var ident = (string)parser.Current.Value;
                if (parser.Peek().TypeId == Program.LParen) // FUNC-INVOKE
                {
                    parser.ConsumeToken(Program.Identifier);
                    return FunctionInvoke(parser, ident);
                }
            }
            return OuterTerm(parser);
        }

        private SyntaxNode OuterTerm(Parser.ParserInstance parser)
        {
            var node = InnerTerm(parser);

            var ops = new[] { Program.Plus, Program.Minus };

            while (ops.Contains(parser.Current.TypeId))
            {
                if (parser.Current.TypeId == Program.Plus)
                {
                    parser.ConsumeToken();
                    node = new AddNode(node, InnerTerm(parser));
                }
                else if (parser.Current.TypeId == Program.Minus)
                {
                    parser.ConsumeToken();
                    node = new SubtractNode(node, InnerTerm(parser));
                }
            }

            // TODO: Comparisons

            return node;
        }

        private SyntaxNode InnerTerm(Parser.ParserInstance parser)
        {
            var node = Factor(parser);

            var ops = new[] { Program.Multiply, Program.Divide };

            while (ops.Contains(parser.Current.TypeId))
            {
                if (parser.Current.TypeId == Program.Multiply)
                {
                    parser.ConsumeToken();
                    node = new MultiplyNode(node, Factor(parser));
                }
                else if (parser.Current.TypeId == Program.Divide)
                {
                    parser.ConsumeToken();
                    node = new DivideNode(node, Factor(parser));
                }
            }

            return node;
        }

        private SyntaxNode FunctionInvoke(Parser.ParserInstance parser, string ident)
        {
            parser.ConsumeToken(Program.LParen);
            var args = new List<SyntaxNode>();
            while (parser.Current.TypeId != Program.RParen)
            {
                if (parser.Current.TypeId == Program.Comma)
                    parser.ConsumeToken();
                args.Add(Expression(parser));
            }
            parser.ConsumeToken();
            return new FunctionInvocationNode(ident, args.ToArray());
        }

        private SyntaxNode Factor(Parser.ParserInstance parser)
        {
            var token = parser.Current;
            parser.ConsumeToken();
            // TODO: Unary-Plus and Unary-Minus
            // TODO: Pre-Increment-By-One and Pre-Decrement-By-One
            if (token.TypeId == Program.Null)
                return new NullNode();
            else if (token.TypeId == Program.Integer) { return new IntegerNode((int)token.Value); }
            else if (token.TypeId == Program.Float) { return new FloatNode((float)token.Value); }
            else if (token.TypeId == Program.True) { return new BooleanNode(true); }
            else if (token.TypeId == Program.False) { return new BooleanNode(false); }
            else if (token.TypeId == Program.String) { return new StringNode((string)token.Value); }
            else if (token.TypeId == Program.LParen)
            {
                var expression = Expression(parser);
                parser.ConsumeToken(Program.RParen);
                return expression;
            }
            else if (token.TypeId == Program.Identifier)
            {
                var result = new VariableNode((string)token.Value);
                // TODO: Post-Increment-By-One and Post-Decrement-By-One!
                return result;
            }
            throw new Exception("bad factor");
        }

        private SyntaxNode Branch(Parser.ParserInstance parser)
        {
            parser.ConsumeToken(Program.LParen);
            var condition = Expression(parser);
            parser.ConsumeToken(Program.RParen);
            SyntaxNode ifTrue = parser.Current.TypeId == Program.LBlock ? Block(parser) : Statement(parser);
            SyntaxNode ifFalse = null;
            if (parser.Current.TypeId == Program.Else)
            {
                parser.ConsumeToken();
                if (parser.Current.TypeId == Program.Branch)
                    ifFalse = Branch(parser);
                else if (parser.Current.TypeId == Program.LBlock)
                    ifFalse = Block(parser);
                else
                    ifFalse = Statement(parser);
            }
            return new BranchNode(condition, ifTrue, ifFalse);
        }
    }
}
