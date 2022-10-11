namespace JLCDB.Utility;
public static class MetricConverter
{
    private static Dictionary<char, double> unitFactors = new Dictionary<char, double>() {
        {'k', 3},
        {'m', -3},
        {'u', -6},
        {'n', -9},
        {'p', -12},
    };

    public static double Normalize(string unit, string normalUnit, double value)
    {
        if (string.Compare(unit, normalUnit, true) == 0)
            return value;

        double factor = unitFactors[unit.ToLower()[0]];

        return value * Math.Pow(10, factor);
    }
}