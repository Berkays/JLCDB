using JLCDB.Utility;

namespace JLCDB.Parser;

internal class MetricParser : ValueParser
{
    const string BASE_PATTERN = @"\s?[+-]?([0-9]*\.?[0-9]*)?([k|m|u|n|p]?{R})\s?";
    public string Unit { get; init; }
    private Regex regex { get; init; }
    public MetricParser(string unit, bool isLowercase = true)
    {
        // this.Unit = isLowercase ? unit.ToLower() : unit;
        this.Unit = unit;
        this.regex = new Regex(BASE_PATTERN.Replace("{R}", this.Unit), RegexOptions.IgnoreCase);
    }
    public override double[] ParseAll(string description)
    {
        var matches = regex.Matches(description);

        return matches.Where(n => string.IsNullOrEmpty(n.Groups[1].Value) == false)
        .Select(n => MetricConverter.Normalize(n.Groups[2].Value, this.Unit, Convert.ToDouble(n.Groups[1].Value))).ToArray();
    }
}