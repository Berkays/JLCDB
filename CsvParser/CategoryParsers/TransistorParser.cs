namespace JLCDB.Parser;

internal static class TransistorParser
{
    public static CategoryParser Create()
    {
        return CategoryParser.Build()
            .Add("Voltage Rating", new MetricParser("V"))
            .Add("Power Rating", new MetricParser("W"))
            .Add("Current", new MetricParser("A"));
    }
}