namespace Dough;

internal class Oven
{
    public Project Project { get; set; }
    public Package[] Packages { get; set; }

    public Oven(Project project, Package[] packages)
    {
        Project = project;
        Packages = packages;
    }
}

internal class Project
{
    public string Name { get; set; }
    public Version Version { get; set; }
    public bool IsLibrary { get => Entry == "" ? true : false; }
    public string Entry { get; set; }

    public Project(string name, Version version, string entry)
    {
        Name = name;
        Version = version;
        Entry = entry;
    }
}

internal class Package
{
    public Version Version { get; set; }
    public string[] Source { get; set; }

    public Package(Version version, string[] source)
    {
        Version = version;
        Source = source;
    }
}

internal class Version
{
    public int Major { get; set; }
    public int Middle { get; set; }
    public int Minor { get; set; }

    public Version(int major, int middle, int minor)
    {
        Major = major;
        Middle = middle;
        Minor = minor;
    }
}