namespace JLCDB.Parser;

internal class ToleranceParser : ValueParser
{
    const string BASE_PATTERN = @"±(\d*.?\d+)%";
    private Regex regex { get; init; }
    public ToleranceParser()
    {
        this.regex = new Regex(BASE_PATTERN);
    }
    public override double[] ParseAll(string description)
    {
        var matches = regex.Matches(description);

        return matches.Where(n => n.Success == true).Select(n => Convert.ToDouble(n.Groups[1].Value)).ToArray();
    }
}