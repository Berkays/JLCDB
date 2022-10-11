namespace JLCDB.Parser;

internal static class InductorParser
{
    public static CategoryParser Create()
    {
        return CategoryParser.Build()
            .Add("Voltage Rating", new MetricParser("V"))
            .Add("Power Rating", new MetricParser("W"))
            .Add("Inductance", new MetricParser("H"))
            .Add("Current", new MetricParser("A"));
    }
}