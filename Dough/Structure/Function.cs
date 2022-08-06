namespace Dough.Structure;

internal record Function(string Identifier, FunctionType Type, Expression[] Expressions);
internal record ExternalFunction(string Identifier, FunctionType Type, string File) : Function(Identifier, Type, null);