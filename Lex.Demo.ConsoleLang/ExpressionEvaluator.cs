using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using Lex.Parsing;

namespace Lex.Demo.ConsoleLang
{
    public class ExpressionEvaluator : Visitor<SyntaxNode>
    {
        private Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>();

        public void AddParametersToVariableScope(FunctionParameter[] parameters, object[] args)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var type = _types.Where(x => x.Value == args[i].GetType()).Select(x => x.Key).FirstOrDefault();
                _variables.Add(parameters[i].Name, new VariableInfo(type, args[i]));
            }
        }

        private List<Function> _functions = new List<Function>
        {
            new Function("op_Add", typeof(int), args => (int)args[0] + (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Add", typeof(float), args => (float)args[0] + (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Add", typeof(float), args => (float)args[0] + (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Add", typeof(float), args => (int)args[0] + (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Add", typeof(string), args => (string)args[0] + args[1].ToString(),
                new []
                {
                    new FunctionParameter("left", typeof(string)),
                    new FunctionParameter("right", typeof(object))
                }),
            new Function("op_Subtract", typeof(int), args => (int)args[0] - (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Subtract", typeof(float), args => (float)args[0] - (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Subtract", typeof(float), args => (float)args[0] - (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Subtract", typeof(float), args => (int)args[0] - (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Multiply", typeof(int), args => (int)args[0] * (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Multiply", typeof(float), args => (float)args[0] * (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Multiply", typeof(float), args => (float)args[0] * (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Multiply", typeof(float), args => (int)args[0] * (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Multiply", typeof(string), args => {
                    var result = string.Empty;
                    var loop = (int)args[1];
                    while (loop-- > 0)
                        result += args[0];
                    return result;
                },
                new []
                {
                    new FunctionParameter("left", typeof(string)),
                    new FunctionParameter("right", typeof(object))
                }),
            new Function("op_Divide", typeof(int), args => (int)args[0] / (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Divide", typeof(float), args => (float)args[0] / (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(float))
                }),
            new Function("op_Divide", typeof(float), args => (float)args[0] / (int)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(float)),
                    new FunctionParameter("right", typeof(int))
                }),
            new Function("op_Divide", typeof(float), args => (int)args[0] / (float)args[1],
                new []
                {
                    new FunctionParameter("left", typeof(int)),
                    new FunctionParameter("right", typeof(float))
                }),
        };

        private Dictionary<string, Type> _types = new Dictionary<string, Type>
        {
            { "void", typeof(void) },
            { "int", typeof(int) },
            { "float", typeof(float) },
            { "bool", typeof(bool) },
            { "string", typeof(string) }
        };
        private List<KeyValuePair<string, string>> _castTable = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("float", "int"),
            new KeyValuePair<string, string>("bool", "int")
        };

        public object Eval(SyntaxNode node)
        {
            try
            {
                return Visit(node);
            }
            catch (Exception error)
            {
                Log(error);
                var inner = error.InnerException;
                while (inner != null)
                {
                    Log(inner);
                    inner = inner.InnerException;
                }
                return null;
            }
        }

        private object Visit(FunctionInvocationNode node)
        {
            var evaledArgs = new List<object>();
            foreach (var arg in node.Arguments)
                evaledArgs.Add(Visit(arg));
            var argTypes = evaledArgs.Select(e => e.GetType());
            var function = GetFunction(node.FunctionName, argTypes.ToArray());
            if (function == null)
                throw new Exception("ugh");
            var result = function.Invoke(evaledArgs.ToArray());
            return result;
        }

        private void Visit(FunctionDefinitionNode node)
        {
            var function = new Function(node.FunctionName, Visit(node.ReturnType), node.Body, node.Parameters.Select(x => new FunctionParameter(x.ParameterName, Visit(x.ParameterType))).ToArray());
            _functions.Add(function);
        }

        private object Visit(BlockNode node)
        {
            object result = null;
            foreach (var child in node.Block)
            {
                if (child is ReturnNode)
                    result = Visit(child);
                else
                    Visit(child);
            }
            return result;
        }

        private void Visit(PrintNode node)
        {
            var expression = Visit(node.Expression);
            WriteLine(expression ?? "NULL");
        }

        private Type Visit(TypeNode node)
        {
            return _types[node.Name];
        }

        private object Visit(ReturnNode node)
        {
            var result = Visit(node.Expression);
            return result;
        }

        private void Visit(VariableDeclarationNode node)
        {
            foreach (var variable in node.Variables)
            {
                if (_variables.ContainsKey(variable.Name))
                {
                    Log(new Exception($"Variable \"{variable.Name}\" already declared!"));
                    continue;
                }
                var type = node.Type.Name;
                var expression = node.Expression != null ? Visit(node.Expression) : null;
                if (node.Type.IsInferred)
                {
                    if (expression == null)
                    {
                        Log(new Exception("Cannot infer variable type without a value!"));
                        continue;
                    }
                    type = _types.Where(x => x.Value == expression.GetType()).Select(x => x.Key).FirstOrDefault();
                    if (string.IsNullOrEmpty(type))
                    {
                        Log(new Exception($"There is no valid type for the expression \"{expression.ToString()}\""));
                        continue;
                    }
                }
                _variables.Add(variable.Name, new VariableInfo(type, expression));
            }
        }

        private object Visit(BranchNode node)
        {
            object result = null;
            var condition = Visit(node.Condition);
            if (condition is bool && (bool)condition)
                result = Visit(node.IfTrue);
            else if (node.IfFalse != null)
                result = Visit(node.IfFalse);
            return result;
        }

        private object Visit(LoopNode node)
        {
            var numberOfLoops = Visit(node.NumberOfLoops);
            if (!(numberOfLoops is int))
            {
                Log(new Exception("Loops can only be given an integer or expression that evaluates to an integer!"));
                return null;
            }

            var currentLoop = (int)numberOfLoops;
            object result = null;
            while (currentLoop-- > 0)
                result = Visit(node.Body);
            return result;
        }

        private void Visit(AssignNode node)
        {
            if (!(node.Left is VariableNode))
            {
                Log(new Exception($"You can only assign values to variables!"));
                return;
            }

            var variableSym = node.Left as VariableNode;
            if (!_variables.ContainsKey(variableSym.Name))
            {
                Log(new InvalidOperationException($"Variable \"{variableSym.Name}\" has not been declared!"));
                return;
            }

            var variable = _variables[variableSym.Name];
            var expression = Visit(node.Right);
            var type = _types.Where(x => x.Value == expression.GetType()).Select(x => x.Key).FirstOrDefault();
            if (variable.Type != type)
            {
                if (_castTable.Any(x => x.Key == variable.Type && x.Value == type))
                {
                    variable.Value = Convert.ChangeType(expression, _types[variable.Type]);
                    return;
                }
                Log(new Exception("Type Mismatch, bro!"));
                return;
            }
            variable.Value = expression;
        }

        private object Visit(TypeOfNode node)
        {
            var expression = Visit(node.Expression);
            return expression.GetType().Name;
        }

        private object Visit(AddNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var function = GetFunction($"op_Add", left.GetType(), left is string ? typeof(object) : right.GetType());
            if (function == null)
            {
                Log(new Exception("ugh"));
                return null;
            }
            var result = function.Invoke(new[] { left, right });
            return result;
        }

        private object Visit(MultiplyNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var function = GetFunction("op_Multiply", left.GetType(), left is string ? typeof(object) : right.GetType());
            if (function == null)
            {
                Log(new Exception("ugh"));
                return null;
            }
            var result = function.Invoke(new[] { left, right });
            return result;
        }

        private object Visit(SubtractNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var function = GetFunction("op_Subtract", left.GetType(), right.GetType());
            if (function == null)
                throw new Exception("ugh");
            var result = function.Invoke(new[] { left, right });
            return result;
        }

        private object Visit(DivideNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var function = GetFunction("op_Divide", left.GetType(), right.GetType());
            if (function == null)
                throw new Exception("ugh");
            var result = function.Invoke(new[] { left, right });
            return result;
        }

        private Function GetFunction(string funcName, params Type[] parameterTypes)
        {
            var functions = _functions.Where(f => f.Name == funcName).ToArray();
            foreach (var func in functions)
            {
                if (func.Parameters.Length != parameterTypes.Length)
                    continue;
                var parameters = func.Parameters.Select(p => p.ParameterType).ToArray();
                var found = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameterTypes[i] != parameters[i])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return func;
            }
            return null;
        }

        private int Visit(IntegerNode node) => node.Value;
        private float Visit(FloatNode node) => node.Value;
        private bool Visit(BooleanNode node) => node.Value;
        private string Visit(StringNode node) => node.Value;

        private object Visit(VariableNode node)
        {
            var variable = _variables.Where(v => v.Key == node.Name).Select(v => v.Value).FirstOrDefault();
            if (variable == null)
                throw new Exception($"No variable named {node.Name}");
            return variable.Value;
        }
    }
}
