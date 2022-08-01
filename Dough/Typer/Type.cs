namespace Dough.Typer;

internal class Type : IEquatable<Type>
{
    public object Value { get; }
    public IEnumerable<(string Identifier, Type Type)>? Parameters { get; }
    
    public Type(Type value, IEnumerable<(string, Type)>? parameters)
    {
        Value = value;
        Parameters = parameters;
    }

    public Type(string value, IEnumerable<(string, Type)>? parameters)
    {
        Value = value;
        Parameters = parameters;
    }

    public override bool Equals(object obj)
    {
        if (obj is not Type other)
            return false;

        return Equals(other);
    }

    public bool Equals(Type? other)
    {
        if (other is null)
            return false;

        if ((Value as string) != (other.Value as string))
            return false;

        if (Parameters is null && other.Parameters is null)
            return true;

        return Parameters?.SequenceEqual(other.Parameters!) ?? false;
    }

    public static bool operator ==(Type a, Type b) => a.Equals(b);
    public static bool operator !=(Type a, Type b) => !a.Equals(b);

    public static readonly Type Void = new Type("void", null);
    public static readonly Type I32 = new Type("i32", null);
    public static readonly Type String = new Type("string", null);
    public static Type Function(Type type, params (string, Type)[] parameters)
    {
        return new Type(type, parameters);
    }
}