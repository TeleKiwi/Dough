namespace Dough.Structure;

internal class Definition
{
    public string Identifier { get; }
    public string Type { get; }

    public Definition(string identifier, string type)
    {
        Identifier = identifier;
        Type = type;
    }
}