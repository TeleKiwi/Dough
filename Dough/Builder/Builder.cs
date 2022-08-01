using Bridge;
using Dough.Structure;

namespace Dough.Builder;

internal class Builder
{
    private static ModuleBuilder unitBuilder = null;
    private static Dictionary<string, RoutineBuilder> functions = new Dictionary<string, RoutineBuilder>();
    private static Dictionary<string, byte> arguments = new Dictionary<string, byte>();
    private static Dictionary<string, Local> variables = new Dictionary<string, Local>();

    private static RoutineBuilder PrintStringTemp = null;

    public static Module BuildUnit(Unit unit)
    {
        unitBuilder = new ModuleBuilder();
        functions = new Dictionary<string, RoutineBuilder>();

        PrintStringTemp = unitBuilder.AddRoutine("PrintString");
        PrintStringTemp.AddParameter(DataType.Pointer);
        CodeBuilder psCode = PrintStringTemp.GetCodeBuilder();
        Label psStart = psCode.AddLabel();
        psCode.Emit(Instruction.PushArg(0));
        psCode.Emit(Instruction.Load(DataType.U16));
        psCode.Emit(Instruction.If(ComparisonKind.Zero, DataType.U16));
        psCode.Emit(Instruction.Return());
        psCode.Emit(Instruction.PushArg(0));
        psCode.Emit(Instruction.Load(DataType.U16));
        psCode.Emit(Instruction.PrintChar(DataType.U16));
        psCode.Emit(Instruction.PushArg(0));
        psCode.Emit(Instruction.PushConst<nuint>(2));
        psCode.Emit(Instruction.Add(DataType.Pointer));
        psCode.Emit(Instruction.PopArg(0));
        psCode.Emit(Instruction.Jump(psStart));

        for (int i = 0; i < unit.Functions.Count(); i++)
            functions[unit.Functions.ElementAt(i).Identifier] = unitBuilder.AddRoutine(unit.Functions.ElementAt(i).Identifier);

        for (int i = 0; i < unit.Functions.Count(); i++)
            BuildFunction(unitBuilder, unit.Functions.ElementAt(i));

        return unitBuilder.CreateModule();
    }

    private static void BuildFunction(ModuleBuilder builder, Function function)
    {
        arguments = new Dictionary<string, byte>();
        variables = new Dictionary<string, Local>();

        RoutineBuilder routineBuilder = functions[function.Identifier];
        routineBuilder.ReturnType = ConvertType(function.Type, true);
        for (int i = 0; i < function.Type.Parameters?.Count(); i++)
        {
            arguments[function.Type.Parameters.ElementAt(i).Identifier] = (byte)i;
            routineBuilder.AddParameter(ConvertType(function.Type.Parameters.ElementAt(i).Type, false));
        }

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
            case ReturnExpression returnExpression:
                BuildReturnExpression(builder, returnExpression, pushResult);
                break;
            case AssignmentExpression assignmentExpression:
                BuildAssignmentExpression(builder, assignmentExpression, pushResult);
                break;
            case BinaryExpression binaryExpression:
                BuildBinaryExpression(builder, binaryExpression, pushResult);
                break;
            case CallExpression callExpression:
                BuildCallExpression(builder, callExpression, pushResult);
                break;
            case NumberExpression numberExpression:
                BuildNumberExpression(builder, numberExpression, pushResult);
                break;
            case StringExpression stringExpression:
                BuildStringExpression(builder, stringExpression, pushResult);
                break;
            case IdentifierExpression identifierExpression:
                BuildIdentifierExpression(builder, identifierExpression, pushResult);
                break;
        }
    }

    private static void BuildPrintExpression(CodeBuilder builder, PrintExpression expression, bool pushResult)
    {
        if (expression.Value.Type! == Type.I32)
        {
            BuildExpression(builder, expression.Value, true);
            builder.Emit(Instruction.Print(DataType.I32));
        }
        else if (expression.Value.Type! == Type.String)
        {
            BuildExpression(builder, expression.Value, true);
            builder.Emit(Instruction.Call(PrintStringTemp));
        }

        if (pushResult)
            builder.Emit(Instruction.PushConst(TypedValue.Create<int>(0)));
    }

    private static void BuildLetExpression(CodeBuilder builder, LetExpression expression, bool pushResult)
    {
        Local local = builder.AddLocal(ConvertType(expression.Type, false));

        BuildExpression(builder, expression.Value, true);
        builder.Emit(Instruction.PopLocal(local));

        if (pushResult)
            builder.Emit(Instruction.PushLocal(local));

        variables[expression.Identifier] = local;
    }

    private static void BuildIfElseExpression(CodeBuilder builder, IfElseExpression expression, bool pushResult)
    {
        Label trueLabel = builder.AddLabel();
        Label endLabel = builder.AddLabel();

        BuildExpression(builder, expression.Case, true);

        builder.Emit(Instruction.If(ComparisonKind.NotZero, DataType.I32));
        builder.Emit(Instruction.Jump(trueLabel));

        BuildExpression(builder, expression.False, true);
        builder.Emit(Instruction.Jump(endLabel));

        builder.MoveLabel(trueLabel);
        BuildExpression(builder, expression.True, true);

        builder.MoveLabel(endLabel);

        if (!pushResult)
            builder.Emit(Instruction.Pop(DataType.I32));
    }

    private static void BuildReturnExpression(CodeBuilder builder, ReturnExpression expression, bool pushResult)
    {
        if (expression.Value != null)
            BuildExpression(builder, expression.Value, true);

        builder.Emit(Instruction.Return());
    }

    private static void BuildAssignmentExpression(CodeBuilder builder, AssignmentExpression expression, bool pushResult)
    {
        BuildExpression(builder, expression.Rhs, true);

        if (expression.Lhs is not IdentifierExpression id)
            throw new Exception("Left side of assignment must be a variable");

        builder.Emit(Instruction.PopLocal(variables[id.Identifier]));

        if (pushResult)
            builder.Emit(Instruction.PushLocal(variables[id.Identifier]));
    }

    private static void BuildBinaryExpression(CodeBuilder builder, BinaryExpression expression, bool pushResult)
    {
        BuildExpression(builder, expression.Lhs, true);
        BuildExpression(builder, expression.Rhs, true);

        switch (expression.Operator)
        {
            case "==":
                builder.Emit(Instruction.Compare(ComparisonKind.Equal, DataType.I32));
                builder.Emit(Instruction.Cast(DataType.I8, DataType.I32));
                break;
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
            case "%":
                builder.Emit(Instruction.Modulo(DataType.I32));
                break;
            default:
                throw new Exception("Unhandled operator");
        }

        if (!pushResult)
            builder.Emit(Instruction.Pop(DataType.I32));
    }

    private static void BuildCallExpression(CodeBuilder builder, CallExpression expression, bool pushResult)
    {
        for (int i = 0; i < expression.Arguments.Count(); i++)
            BuildExpression(builder, expression.Arguments.ElementAt(i), true);

        /*if (arguments.TryGetValue(expression.Identifier, out byte a))
            builder.Emit(Instruction.Call(a));
        else if (variables.TryGetValue(expression.Identifier, out Local v))
            builder.Emit(Instruction.Call(v));*/
        if (functions.TryGetValue(expression.Identifier, out RoutineBuilder f))
            builder.Emit(Instruction.Call(f));
        else
            throw new Exception("Unknown function");

        if (!pushResult && f.ReturnType != DataType.Void)
            builder.Emit(Instruction.Pop(DataType.I32));
    }

    private static void BuildNumberExpression(CodeBuilder builder, NumberExpression expression, bool pushResult)
    {
        if (!pushResult)
            return;

        builder.Emit(Instruction.PushConst(TypedValue.Create<int>(int.Parse(expression.Value))));
    }

    private static void BuildStringExpression(CodeBuilder builder, StringExpression expression, bool pushResult)
    {
        if (!pushResult)
            return;

        Index index = unitBuilder.AddResource(expression.Value + Environment.NewLine + "\0", System.Text.Encoding.Unicode);
        builder.Emit(Instruction.PushResource(index));
    }

    private static void BuildIdentifierExpression(CodeBuilder builder, IdentifierExpression expression, bool pushResult)
    {
        if (!pushResult)
            return;

        if (arguments.TryGetValue(expression.Identifier, out byte a))
            builder.Emit(Instruction.PushArg(a));
        else if (variables.TryGetValue(expression.Identifier, out Local v))
            builder.Emit(Instruction.PushLocal(v));
    }

    private static DataType ConvertType(Type type, bool isFunctionDefinition)
    {
        if (!isFunctionDefinition && type.Parameters is not null)
            return DataType.Pointer;

        return type.Value switch
        {
            "void" => DataType.Void,
            "i32" => DataType.I32,
            "string" => DataType.Pointer,
            _ => throw new Exception("Unhandled type")
        };
    }
}