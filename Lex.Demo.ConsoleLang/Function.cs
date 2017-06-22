using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lex.Demo.ConsoleLang
{
    public class Function
    {
        public string Name { get; internal set; }
        public Type ReturnType { get; internal set; }
        public FunctionParameter[] Parameters { get; internal set; }
        public Func<object[], object> Invoke { get; internal set; }

        public Function(string name, Type returnType, Func<object[], object> invoke, params FunctionParameter[] parameters)
        {
            Name = name;
            ReturnType = returnType;
            Invoke = invoke;
            Parameters = parameters;
        }

        public Function(string name, Type returnType, BlockNode codeTree, params FunctionParameter[] parameters)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
            Invoke = args =>
            {
                var evaluator = new ExpressionEvaluator();
                evaluator.AddParametersToVariableScope(Parameters, args);
                return evaluator.Eval(codeTree);
            };
        }
    }

    public class FunctionParameter
    {
        public string Name { get; internal set; }
        public Type ParameterType { get; internal set; }

        public FunctionParameter(string name, Type parameterType)
        {
            Name = name;
            ParameterType = parameterType;
        }
    }

    public class VariableInfo
    {
        public string Type { get; }
        public object Value { get; set; }

        public VariableInfo(string type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
