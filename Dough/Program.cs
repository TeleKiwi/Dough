using Bridge;
using Dough.Builder;
using Dough.Parser;
using Dough.Structure;
using Dough.Typer;

// ADD UNIT TESTS: maybe ur stuff wont break then

// NOTE FOR TUESDAY: NEATEN PARSER AND BUILDER

// NEATEN THE DAMN BUILDER CLASS PLEASE FOR GODS SAKE (from an athiest)

///
/// TODO: REMOVE VOID
/// void is not functional
/// you uncultured swine!
/// 
/// TODO: CLI ARGUMENTS
/// add arguments for dumping modules
/// cil jit or interpreted, sex-mode
///
/// TODO: FIX FUNCTION ORDERS
/// make it so you can be able call
/// a function before it has been
/// defined
///
/// TODO: FIRST CLASS FUNCTIONS
/// make it so calls (in builder)
/// can be made from functions
/// assigned to variables
/// 
/// TODO: ERROR MESSAGES
/// my porgram no work :(
///
/// TODO: REPLACE PRINT
/// remove print as a keyword and instead
/// use external apis native to the platform
/// remember to update the returned value
/// of print from 0 to the status code
/// 
/// TODO: FIX VARIABLE PRECEDENCE IN BUILDER
/// first check variables, then arguments, then functions
/// 
/// TODO: ADD OPTIONAL NAMING OF PARAMETERS
/// how come when make variable of
/// function type must have smae
/// name :(
/// 
/// TODO: ALIAS
/// who am i :O
/// 
/// TODO: CALLING CONVENTION OF EXTERNALS
/// make sure to no longer hardcode
/// the calling conventions of external
/// functions in the builder
/// 
/// TODO: ADD NEGATIVES
/// why u feeling so down??
///

public class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        args = new string[] { "run", "./Tests/main.do" };
#endif

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
        Console.WriteLine("\"dough help\" - Displays a list of commands.");
        Console.WriteLine("\"dough run [file.do]\" - Runs a dough file through the bridge interpreter.");
        Console.WriteLine("\"dough build [file.do]\" - Compiles a dough file to a bridge file.");
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
        Typer.TypeUnit(unit);
        
        Module output = Builder.BuildUnit(unit);
        return output;
    }

    private static void Run(string[] args)
    {
        if (SourceToModule(args) is not Module module)
            return;

        /*Interpreter interpreter = new Interpreter();
        interpreter.Run(module);*/
        var omg = Module.Compile(module);
        //Module.Dump(module, Console.Out);
        //CILCompiler.DumpModuleIL(Console.Out, module, omg);
        omg.Invoke(null, null);
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