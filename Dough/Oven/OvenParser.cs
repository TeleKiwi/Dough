using Sprache;

namespace Dough;

internal class OvenParser
{
    public static Oven Parse(string source)
    {
        Parser<Oven> parser =
            from project in ParseProject()
            from packages in ParsePackage().Many()
            select new Oven(project, packages.ToArray());

        return parser.Parse(source);
    }

    private static Parser<Project> ParseProject()
    {
        return
            from key in ParseKeyword("[project]")
            from name in ParseProperty("name", ParseString())
            from version in ParseProperty("version", ParseVersion())
            from entry in ParseProperty("entry", ParseString()).Optional()
            select new Project(name, version, entry.GetOrElse(""));
    }

    private static Parser<Package> ParsePackage()
    {
        return
            from key in ParseKeyword("[package]")
            from version in ParseProperty("version", ParseVersion())
            from source in ParseProperty("source", (
                from open in Sprache.Parse.Char('[').Token()
                from strings in ParseString().AtLeastOnce()
                from close in Sprache.Parse.Char(']').Token()
                select strings
            ))
            select new Package(version, source.ToArray());
    }

    private static Parser<T> ParseProperty<T>(string key, Parser<T> value)
    {
        return
            from k in ParseKeyword(key)
            from seperator in Sprache.Parse.Char(':').Token()
            from v in value
            select v;
    }

    private static Parser<string> ParseKeyword(string value)
    {
        return Sprache.Parse.String(value).Token().Where((v) => string.Concat(v) == value).Select(string.Concat);
    }

    private static Parser<string> ParseString()
    {
        return
            from open in Sprache.Parse.Char('\"').Token()
            from value in Sprache.Parse.CharExcept('\"').AtLeastOnce()
            from close in Sprache.Parse.Char('\"').Token()
            select string.Concat(value);
    }

    private static Parser<Version> ParseVersion()
    {
        return
            from maj in Sprache.Parse.Digit.AtLeastOnce()
            from sep1 in Sprache.Parse.Char('.').Token()
            from mid in Sprache.Parse.Digit.AtLeastOnce()
            from sep2 in Sprache.Parse.Char('.').Token()
            from min in Sprache.Parse.Digit.AtLeastOnce()
            select new Version(int.Parse(string.Concat(maj)), int.Parse(string.Concat(mid)), int.Parse(string.Concat(min)));
    }
}
