using Lex.Parsing;

namespace Lex.Demo.ConsoleLang
{
    public class NullNode : SyntaxNode { }

    public class NoOpNode : SyntaxNode { }

    public class ReturnNode : SyntaxNode
    {
        public SyntaxNode Expression { get; }

        public ReturnNode(SyntaxNode expression)
        {
            Expression = expression;
        }
    }

    public class VariableNode : SyntaxNode
    {
        public string Name { get; }

        public VariableNode(string name)
        {
            Name = name;
        }
    }

    public class AssignNode : BinaryOperationNode
    {
        public AssignNode(SyntaxNode left, SyntaxNode right) : base(left, right) { }
    }

    public class VariableDeclarationNode : SyntaxNode
    {
        public VariableNode[] Variables { get; }
        public TypeNode Type { get; }
        public SyntaxNode Expression { get; }

        public VariableDeclarationNode(TypeNode type, params VariableNode[] variables)
        {
            Type = type;
            Variables = variables;
            Expression = null;
        }

        public VariableDeclarationNode(TypeNode type, SyntaxNode expression, params VariableNode[] variables)
        {
            Type = type;
            Variables = variables;
            Expression = expression;
        }
    }

    public class BlockNode : SyntaxNode
    {
        public SyntaxNode[] Block { get; }

        public BlockNode(params SyntaxNode[] block)
        {
            Block = block;
        }
    }

    public class BranchNode : SyntaxNode
    {
        public SyntaxNode Condition { get; }
        public SyntaxNode IfTrue { get; }
        public SyntaxNode IfFalse { get; }

        public BranchNode(SyntaxNode condition, SyntaxNode ifTrue, SyntaxNode ifFalse)
        {
            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }

    public class LoopNode : SyntaxNode
    {
        public SyntaxNode NumberOfLoops { get; }

        public SyntaxNode Body { get; }

        public LoopNode(SyntaxNode numberOfLoops, SyntaxNode body)
        {
            NumberOfLoops = numberOfLoops;
            Body = body;
        }
    }

    public class PrintNode : SyntaxNode
    {
        public SyntaxNode Expression { get; }

        public PrintNode(SyntaxNode expression)
        {
            Expression = expression;
        }
    }

    public class TypeOfNode : SyntaxNode
    {
        public SyntaxNode Expression { get; }

        public TypeOfNode(SyntaxNode expression)
        {
            Expression = expression;
        }
    }

    public class TypeNode : SyntaxNode
    {
        public string Name { get; }

        public TypeNode(string name)
        {
            Name = name;
        }

        public bool IsInferred => string.IsNullOrEmpty(Name);

        public static TypeNode Inferred => new TypeNode(string.Empty);
    }

    public class DataNode<T> : SyntaxNode
    {
        public T Value { get; }

        public DataNode(T value)
        {
            Value = value;
        }
    }

    public class IntegerNode : DataNode<int> { public IntegerNode(int value) : base(value) { } }
    public class FloatNode : DataNode<float> { public FloatNode(float value) : base(value) { } }
    public class BooleanNode : DataNode<bool> { public BooleanNode(bool value) : base(value) { } }
    public class StringNode : DataNode<string> { public StringNode(string value) : base(value) { } }

    public abstract class BinaryOperationNode : SyntaxNode
    {
        public SyntaxNode Left { get; }
        public SyntaxNode Right { get; }

        public BinaryOperationNode(SyntaxNode left, SyntaxNode right)
        {
            Left = left;
            Right = right;
        }
    }

    public class AddNode : BinaryOperationNode { public AddNode(SyntaxNode left, SyntaxNode right) : base(left, right) { } }
    public class SubtractNode : BinaryOperationNode { public SubtractNode(SyntaxNode left, SyntaxNode right) : base(left, right) { } }
    public class MultiplyNode : BinaryOperationNode { public MultiplyNode(SyntaxNode left, SyntaxNode right) : base(left, right) { } }
    public class DivideNode : BinaryOperationNode { public DivideNode(SyntaxNode left, SyntaxNode right) : base(left, right) { } }

    public class FunctionDefinitionNode : SyntaxNode
    {
        public string FunctionName { get; }
        public FunctionParameterDefinitionNode[] Parameters { get; }
        public TypeNode ReturnType { get; }
        public BlockNode Body { get; }

        public FunctionDefinitionNode(string name, TypeNode returnType, FunctionParameterDefinitionNode[] parameters, BlockNode body)
        {
            FunctionName = name;
            ReturnType = returnType;
            Parameters = parameters;
            Body = body;
        }
    }

    public class FunctionParameterDefinitionNode : SyntaxNode
    {
        public string ParameterName { get; }
        public TypeNode ParameterType { get; }

        public FunctionParameterDefinitionNode(string name, TypeNode type)
        {
            ParameterName = name;
            ParameterType = type;
        }
    }

    public class FunctionInvocationNode : SyntaxNode
    {
        public string FunctionName { get; }
        public SyntaxNode[] Arguments { get; }

        public FunctionInvocationNode(string name, params SyntaxNode[] args)
        {
            FunctionName = name;
            Arguments = args;
        }
    }
}