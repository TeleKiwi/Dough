using Bridge;
using Dough.Structure;

namespace Dough.Builder;

internal class BridgeBuilder
{
    private static Dictionary<string, Local> locals = new Dictionary<string, Local>();

    public static Module BuildUnit(Unit unit)
    {
        ModuleBuilder builder = new ModuleBuilder();

        for (int i = 0; i < unit.Functions.Count(); i++)
        {
            Function function = unit.Functions.ElementAt(i);
            BuildFunction(builder, function);
        }

        return builder.CreateModule();
    }

    private static void BuildFunction(ModuleBuilder builder, Function function)
    {
        locals = new Dictionary<string, Local>();
        
        RoutineBuilder routineBuilder = builder.AddRoutine(function.Definition.Identifier);
        routineBuilder.ReturnType = ConvertType(function.Definition.Type);

        CodeBuilder codeBuilder = routineBuilder.GetCodeBuilder();
        for (int i = 0; i < function.Expressions.Count(); i++)
            BuildExpression(codeBuilder, function.Expressions.ElementAt(i), false);

        codeBuilder.Emit(Instruction.Return());
    }

    private static void BuildExpression(CodeBuilder builder, Expression expression, bool pushResult)
    {
        switch (expression)
        {
            case PrintExpression printExpression:
                BuildPrintExpression(builder, printExpression, pushResult);
                break;
            case LetExpression letExpression:
                BuildLetExpression(builder, letExpression, pushResult);
                break;
            case IfElseExpression ifElseExpression:
                BuildIfElseExpression(builder, ifElseExpression, pushResult);
                break;
            case AssignmentExpression assignmentExpression:
                BuildAssignmentExpression(builder, assignmentExpression, pushResult);
                break;
            case BinaryExpression binaryExpression:
                BuildBinaryExpression(builder, binaryExpression, pushResult);
                break;
            case NumberExpression numberExpression:
                BuildNumberExpression(builder, numberExpression, pushResult);
                break;
            case IdentifierExpression identifierExpression:
                BuildIdentifierExpression(builder, identifierExpression, pushResult);
                break;
        }
    }

    private static void BuildPrintExpression(CodeBuilder builder, PrintExpression expression, bool pushResult)
    {
        BuildExpression(builder, expression.Value, true);
        builder.Emit(Instruction.Print(DataType.I32));

        if (pushResult)
            builder.Emit(Instruction.Push(TypedValue.Create<int>(0)));
    }

    private static void BuildLetExpression(CodeBuilder builder, LetExpression expression, bool pushResult)
    {
        locals[expression.Definition.Identifier] = builder.AddLocal(ConvertType(expression.Definition.Type));

        BuildExpression(builder, expression.Value, true);
        builder.Emit(Instruction.Pop(locals[expression.Definition.Identifier]));

        if (pushResult)
            builder.Emit(Instruction.Push(locals[expression.Definition.Identifier]));
    }
    
    private static void BuildIfElseExpression(CodeBuilder builder, IfElseExpression expression, bool pushResult)
    {
        Label falseLabel = builder.AddLabel();
        Label trueLabel = builder.AddLabel();
        Label endLabel = builder.AddLabel();

        builder.Emit(Instruction.If(ComparisonKind.NotZero, DataType.I32));
        builder.Emit(Instruction.Jump(trueLabel));
        
        builder.MoveLabel(falseLabel);
        BuildExpression(builder, expression.False, true);
        builder.Emit(Instruction.Jump(endLabel));

        builder.MoveLabel(trueLabel);
        BuildExpression(builder, expression.True, true);
        builder.Emit(Instruction.Jump(endLabel));

        builder.MoveLabel(endLabel);

        if (!pushResult)
            builder.Emit(Instruction.Pop(DataType.I32));
    }

    private static void BuildAssignmentExpression(CodeBuilder builder, AssignmentExpression expression, bool pushResult)
    {
        BuildExpression(builder, expression.Rhs, true);

        if (expression.Lhs is not IdentifierExpression id)
            throw new Exception("Left side of assignment must be a variable");
        
        builder.Emit(Instruction.Pop(locals[id.Identifier]));

        if (pushResult)
            builder.Emit(Instruction.Push(locals[id.Identifier]));
    }

    private static void BuildBinaryExpression(CodeBuilder builder, BinaryExpression expression, bool pushResult)
    {
        BuildExpression(builder, expression.Lhs, true);
        BuildExpression(builder, expression.Rhs, true);

        switch (expression.Operator)
        {
            case "+":
                builder.Emit(Instruction.Add(DataType.I32));
                break;
            case "-":
                builder.Emit(Instruction.Subtract(DataType.I32));
                break;
            case "*":
                builder.Emit(Instruction.Multiply(DataType.I32));
                break;
            case "/":
                builder.Emit(Instruction.Divide(DataType.I32));
                break;
        }

        if (!pushResult)
            builder.Emit(Instruction.Pop(DataType.I32));
    }

    private static void BuildNumberExpression(CodeBuilder builder, NumberExpression expression, bool pushResult)
    {
        if (pushResult)
            builder.Emit(Instruction.Push(TypedValue.Create<int>(int.Parse(expression.Value))));
    }

    private static void BuildIdentifierExpression(CodeBuilder builder, IdentifierExpression expression, bool pushResult)
    {
        if (pushResult)
            builder.Emit(Instruction.Push(locals[expression.Identifier]));
    }

    private static DataType ConvertType(string type)
    {
        return type switch
        {
            "void" => DataType.Void,
            "i32" => DataType.I32,
            _ => throw new Exception("Unhandled type")
        };
    }
}