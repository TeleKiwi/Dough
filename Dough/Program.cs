using Bridge;
using Dough.Builder;
using Dough.Parser;
using Dough.Structure;

///
/// TODO: GIVE EXPRESSIONS TYPE
/// rememeber to update builder
///
/// TODO: CLI ARGUMENTS
/// add arguments for dumping modules
/// cil jit or interpreted, sex-mode
/// 
/// TODO: OPTIMIZE PUSH RESULT
/// check if not pushing before pushing
/// instead of after so that you dont
/// do the classic push'n pop
/// 
/// TODO: FIX FUNCTION ORDERS
/// make it so you can be able call
/// a function before it has been
/// defined
/// 
/// TODO: NEATEN EVERYTHING
/// please fix builder
/// also see what u can do
/// about all the files in
/// the values folder (omg)
/// 
/// TODO: FIRST CLASS FUNCTIONS
/// make it so calls (in builder)
/// can be made from functions
/// assigned to variables
/// 
/// TODO: OPTIONAL ELSE
/// make elses optional please bruv
/// 
/// TODO: ERROR MESSAGES
/// my porgram no work :(
///

public class Program
{
    public static void Main(string[] args)
    {
        args = new string[] { "build", "./Tests/main.do" };

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

    private static Module? SourceToModule(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Please specify a file to build.");
            return null;
        }

        if (Path.GetExtension(args[1]) != ".do")
        {
            Console.WriteLine("Please specify a \".do\" file.");
            return null;
        }

        string inputPath = args[1];
        string input = File.ReadAllText(inputPath);

        Unit unit = Parser.ParseUnit(input);
        Module output = Builder.BuildUnit(unit);

        return output;
    }

    private static void Run(string[] args)
    {
        if (SourceToModule(args) is not Module module)
            return;
        
        Interpreter interpreter = new Interpreter();
        interpreter.Run(module);
    }

    private static void Build(string[] args)
    {
        if (SourceToModule(args) is not Module module)
            return;

        string outputPath = Path.ChangeExtension(args[1], ".br");

        using StreamWriter file = File.CreateText(outputPath);
        Module.Dump(module, file);
    }

    private static void Invalid()
    {
        Console.WriteLine("Invalid command, try \"dough help\" for available commands.");
    }
}