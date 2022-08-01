namespace Dough.Structure;

internal record Function(string Identifier, Type Type, IEnumerable<Expression> Expressions);