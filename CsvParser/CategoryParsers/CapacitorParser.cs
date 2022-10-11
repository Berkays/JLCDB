namespace JLCDB.Parser;

internal static class CapacitorParser
{
    public static CategoryParser Create()
    {
        return CategoryParser.Build()
            .Add("Voltage Rating", new MetricParser("V"))
            .Add("Capacitance", new MetricParser("F"))
            .Add("Tolerance", new ToleranceParser());
    }
}