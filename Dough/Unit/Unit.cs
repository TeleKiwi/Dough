namespace Dough.Structure;

internal class Unit
{
    public IEnumerable<Function> Functions { get; }

    public Unit(IEnumerable<Function> functions)
    {
        Functions = functions;
    }
}