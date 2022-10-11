
namespace JLCDB.API.Models;
public record class SocketCommand(Command Command, SocketData Data);


public enum Command
{
    GetMainCategories = 0,
    GetBaseProperties = 1,
    GetExtraProperties = 2,
    GetResults = 3
}

public record class SocketData(string MainCategory, string Query, Dictionary<string, string> ClientMap);