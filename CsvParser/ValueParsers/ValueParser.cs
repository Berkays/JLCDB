namespace JLCDB.Parser;

abstract class ValueParser
{
    public Nullable<double> ParseSingle(string description)
    {
        var matches = ParseAll(description);

        if (matches != null && matches.Length > 0)
            return matches[0];

        return null;
    }
    public abstract double[] ParseAll(string description);
}