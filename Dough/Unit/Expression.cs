namespace Dough.Structure;

internal abstract record Expression();

internal record PrintExpression(Expression Value) : Expression();
internal record LetExpression(Definition Definition, Expression Value) : Expression();
internal record NumberExpression(string Value) : Expression();
internal record IdentifierExpression(string Value) : Expression();