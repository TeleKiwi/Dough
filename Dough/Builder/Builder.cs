using Dough.Structure;

namespace Dough.Builder;

internal class Builder
{
    public static string BuildUnit(Unit unit)
    {
        StringWriter writer = new StringWriter();

        for (int i = 0; i < unit.Functions.Count(); i++)
        {
            Function function = unit.Functions.ElementAt(i);
            BuildFunction(writer, function);
        }

        return writer.ToString();
    }

    private static void BuildFunction(StringWriter writer, Function function)
    {
        writer.WriteLine($"routine {ConvertType(function.Definition.Type)} {function.Definition.Identifier}");
        writer.WriteLine('{');

        for (int i = 0; i < function.Expressions.Count(); i++)
            if (function.Expressions.ElementAt(i) is LetExpression letExpression)
                BuildLetExpressionDeclare(writer, letExpression);

        for (int i = 0; i < function.Expressions.Count(); i++)
            BuildExpression(writer, function.Expressions.ElementAt(i), false);

        writer.WriteLine("return");
        writer.WriteLine('}');
    }

    private static void BuildExpression(StringWriter writer, Expression expression, bool pushResult)
    {
        switch (expression)
        {
            case PrintExpression printExpression:
                BuildPrintExpression(writer, printExpression, pushResult);
                break;
            case LetExpression letExpression:
                BuildLetExpression(writer, letExpression, pushResult);
                break;
            case NumberExpression numberExpression:
                BuildNumberExpression(writer, numberExpression, pushResult);
                break;
            case IdentifierExpression identifierExpression:
                BuildIdentifierExpression(writer, identifierExpression, pushResult);
                break;
        }
    }

    private static void BuildPrintExpression(StringWriter writer, PrintExpression expression, bool pushResult)
    {
        BuildExpression(writer, expression.Value, true);
        writer.WriteLine("print.i32");

        if (pushResult)
            writer.WriteLine("push.const.i32 0");
    }

    private static void BuildLetExpression(StringWriter writer, LetExpression expression, bool pushResult)
    {
        BuildExpression(writer, expression.Value, true);
        writer.WriteLine($"pop.local {expression.Definition.Identifier}");

        if (pushResult)
            writer.WriteLine($"push.local {expression.Definition.Identifier}");
    }

    private static void BuildLetExpressionDeclare(StringWriter writer, LetExpression expression)
    {
        writer.WriteLine($"local.{ConvertType(expression.Definition.Type)} {expression.Definition.Identifier}");
    }

    private static void BuildNumberExpression(StringWriter writer, NumberExpression expression, bool pushResult)
    {
        if (pushResult)
            writer.WriteLine($"push.const.i32 {expression.Value}");
    }

    private static void BuildIdentifierExpression(StringWriter writer, IdentifierExpression expression, bool pushResult)
    {
        if (pushResult)
            writer.WriteLine($"push.local {expression.Identifier}");
    }

    private static string ConvertType(string type)
    {
        return type switch
        {
            "void" => "",
            "i32" => "i32",
            _ => throw new Exception("Unhandled type")
        };
    }
}