namespace Dough.Structure;

internal abstract class Expression
{
    public Type? Type { get; set; }
}

internal class PrintExpression : Expression
{
    public Expression Value { get; set; }

    public PrintExpression(Expression value)
    {
        Value = value;
    }
}

internal class LetExpression : Expression
{
    public string Identifier { get; set; }
    public Type? Annotation { get; set; }
    public Expression Value { get; set; }

    public LetExpression(string identifier, Type? annotation, Expression value)
    {
        Identifier = identifier;
        Annotation = annotation;
        Value = value;
    }
}

internal class IfElseExpression : Expression
{
    public Expression Case { get; set; }
    public Expression True { get; set; }
    public Expression False { get; set; }
    
    public IfElseExpression(Expression @case, Expression @true, Expression @false)
    {
        Case = @case;
        True = @true;
        False = @false;
    }
}

internal class ReturnExpression : Expression
{
    public Expression? Value { get; set; }
    
    public ReturnExpression(Expression? value)
    {
        Value = value;
    }
}

internal class AssignmentExpression : Expression
{
    public Expression Lhs { get; set; }
    public Expression Rhs { get; set; }

    public AssignmentExpression(Expression lhs, Expression rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }
}

internal class BinaryExpression : Expression
{
    public Expression Lhs { get; set; }
    public string Operator { get; set; }
    public Expression Rhs { get; set; }

    public BinaryExpression(Expression lhs, string @operator, Expression rhs)
    {
        Lhs = lhs;
        Operator = @operator;
        Rhs = rhs;
    }
}

internal class CallExpression : Expression
{
    public string Identifier { get; set; }
    public IEnumerable<Expression> Arguments { get; set; }
    
    public CallExpression(string identifier, IEnumerable<Expression> arguments)
    {
        Identifier = identifier;
        Arguments = arguments;
    }
}

internal class NumberExpression : Expression
{
    public string Value { get; set; }
    
    public NumberExpression(string value)
    {
        Value = value;
    }
}

internal class StringExpression : Expression
{
    public string Value { get; set; }
    
    public StringExpression(string value)
    {
        Value = value;
    }
}

internal class IdentifierExpression : Expression
{
    public string Identifier { get; set; }

    public IdentifierExpression(string identifier)
    {
        Identifier = identifier;
    }
}