namespace Dough.Structure;

internal abstract record Expression();

internal record PrintExpression(Expression Value) : Expression();
internal record LetExpression(Definition Definition, Expression Value) : Expression();
internal record IfElseExpression(Expression Case, Expression True, Expression False) : Expression();
internal record ReturnExpression(Expression? Value) : Expression();
internal record AssignmentExpression(Expression Lhs, Expression Rhs) : Expression();
internal record BinaryExpression(Expression Lhs, string Operator, Expression Rhs) : Expression();
internal record CallExpression(string Identifier, IEnumerable<Expression> Arguments) : Expression();
internal record NumberExpression(string Value) : Expression();
internal record IdentifierExpression(string Identifier) : Expression();