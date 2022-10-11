using MongoDB.Bson.Serialization.Attributes;

[BsonNoId]
[BsonIgnoreExtraElements]
public record class BaseComponent(
     string LCSCPart,
     string FirstCategory,
     string SecondCategory,
     string Package,
     LibraryType LibraryType,
     string Description,
     string Datasheet,
     double Price

);
public enum LibraryType
{
    Basic = 0,
    Extended = 1
}