namespace Dough;

internal class OvenWriter
{
    public static void Write(Oven oven, StreamWriter writer)
    {
        WriteProject(oven.Project, writer);
        foreach (Package p in oven.Packages)
            WritePackage(p, writer);
    }

    private static void WriteProject(Project project, StreamWriter writer)
    {
        writer.WriteLine("[project]");

        writer.WriteLine($"name: \"{project.Name}\"");

        writer.Write("version: ");
        WriteVersion(project.Version, writer);
        writer.WriteLine();

        if (project.Entry != "")
            writer.WriteLine($"entry: \"{project.Entry}\"");
    }

    private static void WritePackage(Package package, StreamWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("[package]");

        writer.Write("version: ");
        WriteVersion(package.Version, writer);
        writer.WriteLine();

        writer.WriteLine("source: [");
        foreach (string s in package.Source)
            writer.WriteLine($"  \"{s}\"");
        writer.WriteLine("]");
    }

    private static void WriteVersion(Version version, StreamWriter writer)
    {
        writer.Write(version.Major);
        writer.Write('.');
        writer.Write(version.Middle);
        writer.Write('.');
        writer.Write(version.Minor);
    }
}
