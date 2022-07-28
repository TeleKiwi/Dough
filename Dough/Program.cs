using Bridge;
using Dough.Builder;
using Dough.Parser;
using Dough.Structure;

///
/// TODO: GIVE EXPRESSIONS TYPE
/// rememeber to update builder
///

public class Program
{
    public static void Main(string[] args)
    {
        args = new string[] { "run", "./Tests/main.do" };

        if (args.Length == 0)
        {
            Console.WriteLine("Welcome to the Dough programming language! Run \"dough help\" for more info.");
            return;
        }

        switch (args[0])
        {
            case "help":
                Help();
                break;
            case "run":
                Run(args);
                break;
            case "build":
                Build(args);
                break;
            default:
                Invalid();
                break;
        }
    }

    private static void Help()
    {
        Console.WriteLine("\"dough help - Displays a list of commands.\"");
        Console.WriteLine("\"dough run [file.do] - Runs a dough file through the bridge interpreter.\"");
        Console.WriteLine("\"dough build [file.do] - Compiles a dough file to a bridge file.\"");
    }

    private static void Build(string[] args)
    {
        Console.WriteLine("Build is currently not stable and cannot be run. Please try \"dough run [file.do]\" instead.");
        return;

        if (args.Length < 2)
        {
            Console.WriteLine("Please specify a file to build.");
            return;
        }

        if (Path.GetExtension(args[1]) != ".do")
        {
            Console.WriteLine("Please specify a \".do\" file.");
            return;
        }

        string inputPath = args[1];
        string input = File.ReadAllText(inputPath);

        Unit unit = Parser.ParseUnit(input);
        string output = Builder.BuildUnit(unit);

        string outputPath = Path.ChangeExtension(args[1], ".br");
        File.WriteAllText(outputPath, output);
    }

    private static void Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Please specify a file to build.");
            return;
        }

        if (Path.GetExtension(args[1]) != ".do")
        {
            Console.WriteLine("Please specify a \".do\" file.");
            return;
        }

        string inputPath = args[1];
        string input = File.ReadAllText(inputPath);

        Unit unit = Parser.ParseUnit(input);
        Module output = BridgeBuilder.BuildUnit(unit);

        Interpreter interpreter = new Interpreter();
        interpreter.Run(output);
    }

    private static void Invalid()
    {
        Console.WriteLine("Invalid command, try \"dough help\" for available commands.");
    }
}