namespace JLCDB.API.Models;
public record class ComponentProperty(PropertyType Type, PropertyScope Scope, string Name);

public enum PropertyType
{
    Ordinal = 0,
    Numerical = 1,
    Text = 2
}

public enum PropertyScope
{
    Base = 0,
    Extra = 1
}