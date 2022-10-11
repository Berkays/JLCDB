namespace JLCDB.Parser;

internal static class ResistorParser
{
    public static CategoryParser Create()
    {
        return CategoryParser.Build()
            .Add("Voltage Rating", new MetricParser("V"))
            .Add("Power Rating", new MetricParser("W"))
            .Add("Resistance", new MetricParser("Ω"))
            .Add("Tolerance", new ToleranceParser());
    }
}