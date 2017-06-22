using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lex.Parsing
{
    public abstract class Visitor<T> where T : class
    {
        private readonly Dictionary<Type, MethodInfo> _methods;

        private List<Exception> _errors = new List<Exception>();
        public Exception[] Errors => _errors.ToArray();
        public bool IsSuccess => _errors.Count == 0;

        public Visitor()
        {
            var type = GetType();
            _methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.Name.StartsWith("Visit") && m.GetParameters().Length == 1)
                .ToDictionary(m => m.GetParameters().First().ParameterType);
        }

        protected object Visit(T node)
        {
            if (!_methods.ContainsKey(node.GetType()))
                return null;
            return _methods[node.GetType()].Invoke(this, new[] { node });
        }

        public void Log(Exception error) => _errors.Add(error);

        public void ClearErrors() => _errors.Clear();
    }
}