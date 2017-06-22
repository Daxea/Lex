namespace Lex
{
    public class Token
    {
        public int TypeId { get; }
        public object Value { get; }

        public bool IsEmpty => TypeId == int.MinValue;

        public Token(int typeId)
        {
            TypeId = typeId;
            Value = null;
        }

        public Token(int typeId, object value)
        {
            TypeId = typeId;
            Value = value;
        }

        public static Token Empty => new Token(int.MinValue);
    }
}