namespace Dough.Structure;

internal class Function
{
    public Definition Definition { get; }
    public IEnumerable<Expression> Expressions { get; }

    public Function(Definition definition, IEnumerable<Expression> expressions)
    {
        Definition = definition;
        Expressions = expressions;
    }
}