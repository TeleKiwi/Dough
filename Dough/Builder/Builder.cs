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
            BuildExpression(writer, function.Expressions.ElementAt(i));

        writer.WriteLine("return");
        writer.WriteLine('}');
    }

    private static void BuildExpression(StringWriter writer, Expression expression)
    {
        switch (expression)
        {
            case PrintExpression printExpression:
                BuildPrintExpression(writer, printExpression);
                break;
            case LetExpression letExpression:
                BuildLetExpression(writer, letExpression);
                break;
            case NumberExpression numberExpression:
                BuildNumberExpression(writer, numberExpression);
                break;
            case IdentifierExpression identifierExpression:
                BuildIdentifierExpression(writer, identifierExpression);
                break;
        }
    }

    private static void BuildPrintExpression(StringWriter writer, PrintExpression expression)
    {
        BuildExpression(writer, expression.Value);
        writer.WriteLine("print.i32");
    }

    private static void BuildLetExpression(StringWriter writer, LetExpression expression)
    {
        BuildExpression(writer, expression.Value);
        writer.WriteLine($"pop.local {expression.Definition.Identifier}");
    }

    private static void BuildLetExpressionDeclare(StringWriter writer, LetExpression expression)
    {
        writer.WriteLine($"local.{ConvertType(expression.Definition.Type)} {expression.Definition.Identifier}");
    }

    private static void BuildNumberExpression(StringWriter writer, NumberExpression expression)
    {
        writer.WriteLine($"push.const.i32 {expression.Value}");
    }

    private static void BuildIdentifierExpression(StringWriter writer, IdentifierExpression expression)
    {
        writer.WriteLine($"push.local {expression.Value}");
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