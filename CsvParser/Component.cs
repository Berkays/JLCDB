namespace JLCDB.Parser;

internal record class Component(
    string LCSCPart,
    string FirstCategory,
    string SecondCategory,
    string Package,
    string Manufacturer,
    string LibraryType,
    string Description,
    string Datasheet,
    double Price,
    uint Stock
    );