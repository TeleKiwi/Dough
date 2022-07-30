namespace Dough.Structure;

internal struct Type
{
    public string Value { get; }
    public IEnumerable<Definition>? Parameters { get; }

    public Type(string value, IEnumerable<Definition>? parameters)
    {
        Value = value;
        Parameters = parameters;
    }
}