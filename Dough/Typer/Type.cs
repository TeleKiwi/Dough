namespace Dough;

internal abstract class Type
{
    public static Type Unknown()
    {
        return new CoreType("unknown");
    }

    public static Type Void()
    {
        return new CoreType("void");
    }

    public static Type I32()
    {
        return new CoreType("i32");
    }

    public static Type String()
    {
        return new CoreType("string");
    }

    public static Type Function(Type value, (string id, Type type)[] arguments)
    {
        return new FunctionType(value, arguments);
    }
}

internal class CoreType : Type
{
    public string Value { get; set; }

    public CoreType(string value)
    {
        Value = value;
    }
}

internal class FunctionType : Type
{
    public Type Value { get; set; }
    
    public (string id, Type type)[] Arguments { get; set; }

    public FunctionType(Type value, (string id, Type type)[] arguments)
    {
        Value = value;
        Arguments = arguments;
    }
}