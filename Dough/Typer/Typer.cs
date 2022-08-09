namespace Dough;

internal class Typer
{
    private static Type current = Type.Unknown();
    private static Dictionary<string, Type> functions = new Dictionary<string, Type>();
    private static Dictionary<string, Type> variables = new Dictionary<string, Type>();

    public static void TypeUnit(Unit unit)
    {
        functions = new Dictionary<string, Type>();
        
        foreach (Function function in unit.Functions)
            functions[function.Identifier] = function.Type;
        
        foreach (Function function in unit.Functions.Where((f) => f is not ExternalFunction))
            TypeFunction(function);
    }
    
    private static void TypeFunction(Function function)
    {
        current = function.Type;
        variables = new Dictionary<string, Type>();
        
        foreach ((string id, Type type) argument in function.Type.Arguments)
            variables[argument.id] = argument.type;

        foreach (Expression expression in function.Expressions)
            TypeExpression(expression);
    }

    private static Type TypeExpression(Expression expression)
    {
        return expression switch
        {
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

    private static Type TypeLetExpression(LetExpression expression)
    {
        if (TypeEquals(expression.Annotation, Type.Unknown()))
        {
            variables[expression.Identifier] = TypeExpression(expression.Value);

            expression.Type = variables[expression.Identifier];
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.Value), expression.Annotation))
        {
            variables[expression.Identifier] = expression.Annotation;

            expression.Type = variables[expression.Identifier];
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static Type TypeIfElseExpression(IfElseExpression expression)
    {
        // check for boolean condition
        
        if (TypeEquals(TypeExpression(expression.True), TypeExpression(expression.False)))
        {
            expression.Type = expression.True.Type;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.True), TypeExpression(expression.False)))
        {
            expression.Type = expression.False.Type;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.False), TypeExpression(expression.True)))
        {
            expression.Type = expression.True.Type;
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static Type TypeReturnExpression(ReturnExpression expression)
    {
        if (TypeEquals((current as FunctionType)!.Value, Type.Void()) && expression.Value is null)
        {
            expression.Type = current;
            return expression.Type;
        }
        
        if (TypeEquals(current, TypeExpression(expression.Value!)))
        {
            expression.Type = current;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.Value!), current))
        {
            expression.Type = current;
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static Type TypeAssignmentExpression(AssignmentExpression expression)
    {
        if (TypeEquals(TypeExpression(expression.Lhs), TypeExpression(expression.Rhs)))
        {
            expression.Type = expression.Lhs.Type;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.Rhs), TypeExpression(expression.Lhs)))
        {
            expression.Type = expression.Lhs.Type;
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static Type TypeBinaryExpression(BinaryExpression expression)
    {
        if (!TypeIsNumber(TypeExpression(expression.Lhs)) && TypeIsNumber(TypeExpression(expression.Rhs)))
            throw new ForgorException();
        
        if (TypeEquals(TypeExpression(expression.Lhs), TypeExpression(expression.Rhs)))
        {
            expression.Type = expression.Lhs.Type;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.Lhs), TypeExpression(expression.Rhs)))
        {
            expression.Type = expression.Rhs.Type;
            return expression.Type;
        }

        if (TypeConverts(TypeExpression(expression.Rhs), TypeExpression(expression.Lhs)))
        {
            expression.Type = expression.Lhs.Type;
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static Type TypeCallExpression(CallExpression expression)
    {
        FunctionType function;
        if (functions.TryGetValue(expression.Identifier, out Type? f))
            function = (f as FunctionType)!;
        else if (variables.TryGetValue(expression.Identifier, out Type? v))
            function = (v as FunctionType)!;
        else
            throw new ForgorException();

        if (expression.Arguments.Length != function.Arguments.Length)
            throw new ForgorException();

        for (int i = 0; i < function.Arguments.Length; i++)
        {
            Type expressionArg = TypeExpression(expression.Arguments[i]);
            Type functionArg = function.Arguments[i].type;

            if (!TypeEquals(expressionArg, functionArg) && !TypeConverts(expressionArg, functionArg))
                throw new ForgorException();
        }

        expression.Type = function.Value;
        return expression.Type;
    }

    private static Type TypeNumberExpression(NumberExpression expression)
    {
        expression.Type = Type.I32();
        return expression.Type;
    }

    private static Type TypeStringExpression(StringExpression expression)
    {
        expression.Type = Type.String();
        return expression.Type;
    }

    private static Type TypeIdentifierExpression(IdentifierExpression expression)
    {
        if (functions.TryGetValue(expression.Identifier, out Type? f))
        {
            expression.Type = f;
            return expression.Type;
        }
        
        if (variables.TryGetValue(expression.Identifier, out Type? v))
        {
            expression.Type = v;
            return expression.Type;
        }

        throw new ForgorException();
    }

    private static bool TypeIsNumber(Type type)
    {
        return TypeEquals(type, Type.I32());
    }

    private static bool TypeConverts(Type from, Type to)
    {
        return false; // forgor :skull:
    }

    private static bool TypeEquals(Type type1, Type type2)
    {
        if (type1 is CoreType type1Core && type2 is CoreType type2Core)
            return type1Core.Value == type2Core.Value;
        
        if (type1 is FunctionType type1Function && type2 is FunctionType type2Function)
        {
            if (!TypeEquals(type1Function.Value, type2Function.Value))
                return false;

            if (type1Function.Arguments.Length != type2Function.Arguments.Length)
                return false;

            for (int i = 0; i < type1Function.Arguments.Length; i++)
            {
                Type function1Arg = type1Function.Arguments[i].type;
                Type function2Arg = type2Function.Arguments[i].type;

                if (!TypeEquals(function1Arg, function2Arg))
                    return false;
            }

            return true;
        }

        return false;
    }
}

public class ForgorException : Exception { }