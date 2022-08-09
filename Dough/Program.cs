using Bridge;

namespace Dough;

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
            case "create":
                Create(args);
                break;
            case "import":
                Import(args);
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
        
        Console.WriteLine("\"dough create [name]\" - Creates a new dough project.");
        Console.WriteLine("\"dough import [directory]\" - Imports a package.");
        
        Console.WriteLine("\"dough run\" - Runs a dough project using dotnet CIL.");
        Console.WriteLine("\"dough build\" - Compiles a dough project to a bridge file.");
    }

    private static void Create(string[] args)
    {
        if (args.Length == 1)
        {
            Console.WriteLine("Please specify a name for your project.");
            return;
        }

        string directory = Environment.CurrentDirectory;

        using FileStream mainFile = File.Create(Path.Combine(directory, "main.do"));
        using StreamWriter main = new StreamWriter(mainFile);
        
        using FileStream configFile = File.Create(Path.Combine(directory, ".oven"));
        using StreamWriter config = new StreamWriter(configFile);

        main.Write(@"use std:io

def main: void() {
  print(""hello world!"")
}");

        config.Write(@$"[project]
name: ""{args[1]}""
version: 0.0.1
entry: ""./main.do""

[package]
version: 0.0.0
source: [
  ""https://github.com/thedoughlang/std""
]");

        Console.WriteLine();
        Console.WriteLine("Successfully created project.");
    }

    private static void Import(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Please specify a package to import.");
            return;
        }

        string directory = Environment.CurrentDirectory;

        string configFile = File.ReadAllText(Path.Combine(directory, ".oven"));
        Oven config = OvenParser.Parse(configFile);
        
        Package p = new Package(new Version(0, 0, 0), new string[] { args[1] });
        config.Packages = config.Packages.Append(p).ToArray();

        using StreamWriter configWriter = new StreamWriter(Path.Combine(directory, ".oven"));
        OvenWriter.Write(config, configWriter);

        Console.WriteLine();
        Console.WriteLine("Successfully imported package.");
    }

    private static void Run(string[] args)
    {
        string directory = Environment.CurrentDirectory;

        string configFile = File.ReadAllText(Path.Combine(directory, ".oven"));
        Oven config = OvenParser.Parse(configFile);

        if (config.Packages.Length != 0)
        {
            Console.WriteLine("Dough does not currently support packages");
            return;
        }
        
        if (SourceToModule(config.Project.Entry) is not Module module)
            return;

        var cil = Module.Compile(module);
        cil.Invoke(null, null);

        Console.WriteLine();
        Console.WriteLine("Successfully ran project.");
    }

    private static void Build(string[] args)
    {
        string directory = Environment.CurrentDirectory;

        string configFile = File.ReadAllText(Path.Combine(directory, ".oven"));
        Oven config = OvenParser.Parse(configFile);

        if (config.Packages.Length != 0)
        {
            Console.WriteLine("Dough does not currently support packages");
            return;
        }

        if (SourceToModule(config.Project.Entry) is not Module module)
            return;

        string outputPath = Path.Combine(Path.GetDirectoryName(config.Project.Entry), $"{config.Project.Name}.br");

        using StreamWriter file = File.CreateText(outputPath);
        Module.Dump(module, file);

        Console.WriteLine();
        Console.WriteLine("Successfully built project.");
    }

    private static void Invalid()
    {
        Console.WriteLine("Invalid command, try \"dough help\" for available commands.");
    }
    
    private static Module? SourceToModule(string source)
    {
        if (!File.Exists(source))
        {
            Console.WriteLine("Please specify a valid file.");
            return null;
        }

        if (Path.GetExtension(source) != ".do")
        {
            Console.WriteLine("Please specify a \".do\" file.");
            return null;
        }

        string input = File.ReadAllText(source);
        Unit unit = Parser.ParseUnit(input);
        
        Typer.TypeUnit(unit);

        Module output = Builder.BuildUnit(unit);
        return output;
    }
}