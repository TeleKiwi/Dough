using Dough.Structure;

namespace Dough.Typer;

internal class Typer
{
    private static Dictionary<string, Type> functions = null;
    private static Dictionary<string, Type> variables = null;

    public static void TypeUnit(Unit unit)
    {
        functions = new Dictionary<string, Type>();
        
        foreach (Function function in unit.Functions)
            functions[function.Identifier] = function.Type;
        
        foreach (Function function in unit.Functions)
            TypeFunction(function);
    }
    
    private static void TypeFunction(Function function)
    {
        variables = new Dictionary<string, Type>();

        if (function.Type.Parameters is null)
            throw new Exception("A function must have parameters");

        foreach ((string id, Type type) parameter in function.Type.Parameters!)
            variables[parameter.id] = parameter.type;

        foreach (Expression expression in function.Expressions)
            TypeExpression(expression);
    }

    private static Type TypeExpression(Expression expression)
    {
        return expression switch
        {
            PrintExpression printExpression => TypePrintExpression(printExpression),
            LetExpression letExpression => TypeLetExpression(letExpression),
            IfElseExpression ifElseExpression => TypeIfElseExpression(ifElseExpression),
            ReturnExpression returnExpression => TypeReturnExpression(returnExpression),
            AssignmentExpression assignmentExpression => TypeAssignmentExpression(assignmentExpression),
            BinaryExpression binaryExpression => TypeBinaryExpression(binaryExpression),
            CallExpression callExpression => TypeCallExpression(callExpression),
            NumberExpression numberExpression => TypeNumberExpression(numberExpression),
            StringExpression stringExpression => TypeStringExpression(stringExpression),
            IdentifierExpression identifierExpression => TypeIdentifierExpression(identifierExpression),
            _ => throw new Exception("Unknown expression type")
        };
    }

    private static Type TypePrintExpression(PrintExpression expression)
    {
        expression.Type = TypeExpression(expression.Value);
        return expression.Type;
    }

    private static Type TypeLetExpression(LetExpression expression)
    {
        if (expression.Annotation is not null)
            if (expression.Annotation != TypeExpression(expression.Value))
                throw new Exception("Type mismatch");
            else
                variables[expression.Identifier] = expression.Annotation;
        else
            variables[expression.Identifier] = TypeExpression(expression.Value);

        expression.Type = variables[expression.Identifier];
        return expression.Type;
    }

    private static Type TypeIfElseExpression(IfElseExpression expression)
    {
        Type conditionType = TypeExpression(expression.Case);
        Type trueType = TypeExpression(expression.True);
        Type falseType = TypeExpression(expression.False);
        
        /*if (conditionType != Type.Bool)
            throw new Exception("If condition must be boolean");*/
        if (trueType != falseType)
            throw new Exception("If cases must both be the same type");

        expression.Type = trueType;
        return expression.Type;
    }

    private static Type TypeReturnExpression(ReturnExpression expression)
    {
        if (expression.Value is not null)
            expression.Type = TypeExpression(expression.Value);
        else
            expression.Type = Type.Void;

        return expression.Type;
    }

    private static Type TypeAssignmentExpression(AssignmentExpression expression)
    {
        Type lhsType = TypeExpression(expression.Lhs);
        Type rhsType = TypeExpression(expression.Rhs);

        if (lhsType != rhsType)
            throw new Exception("Both sides of assignment must be the same type");

        expression.Type = lhsType;
        return expression.Type;
    }

    private static Type TypeBinaryExpression(BinaryExpression expression)
    {
        Type lhsType = TypeExpression(expression.Lhs);
        Type rhsType = TypeExpression(expression.Rhs);

        if (lhsType != rhsType)
            throw new Exception("Both sides of binary expression must be the same type");
        if (lhsType != Type.I32)
            throw new Exception("Binary expression must be of type int");

        expression.Type = lhsType;
        return expression.Type;
    }

    private static Type TypeCallExpression(CallExpression expression)
    {
        Type functionType = functions[expression.Identifier];

        if (functionType.Parameters is null)
            throw new Exception("Call must be to a function");
        if (functionType.Parameters?.Count() != expression.Arguments.Count())
            throw new Exception("Function call has wrong number of arguments");
        for (int i = 0; i < expression.Arguments.Count(); i++)
        {
            Type argumentType = TypeExpression(expression.Arguments.ElementAt(i));
            Type parameterType = functionType.Parameters.ElementAt(i).Type;
            
            if (argumentType != parameterType)
                throw new Exception("Function argument types must match");
        }

        expression.Type = functionType;
        return expression.Type;
    }

    private static Type TypeNumberExpression(NumberExpression expression)
    {
        expression.Type = Type.I32;
        return expression.Type;
    }

    private static Type TypeStringExpression(StringExpression expression)
    {
        expression.Type = Type.String;
        return expression.Type;
    }

    private static Type TypeIdentifierExpression(IdentifierExpression expression)
    {
        expression.Type = variables[expression.Identifier];
        return expression.Type;
    }
}