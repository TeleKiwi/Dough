namespace Dough.Structure;

internal class Definition
{
    public string Identifier { get; }
    public Type Type { get; }

    public Definition(string identifier, Type type)
    {
        Identifier = identifier;
        Type = type;
    }
}