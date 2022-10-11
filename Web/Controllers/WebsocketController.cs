using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

using JLCDB.API.Models;
using JLCDB.API.Services;

namespace JLCDB.API.Controllers;
[ApiController]
[Route("[controller]")]
public class WebSocketsController : ControllerBase
{
    private readonly ILogger<WebSocketsController> _logger;

    private readonly ComponentsService _componentsService;

    public WebSocketsController(ILogger<WebSocketsController> logger, ComponentsService componentsService)
    {
        _logger = logger;
        _componentsService = componentsService;
    }

    [HttpGet("/wsconn")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.Log(LogLevel.Information, "WebSocket connection established");

            await SocketLoop(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task<byte[]> Action(byte[] buffer, int BufferLength)
    {
        using var ms = new MemoryStream(buffer, 0, BufferLength);
        var result = await JsonSerializer.DeserializeAsync<SocketCommand>(ms);

        if (result == null)
            return Encoding.UTF8.GetBytes("{ \"error\": 0 }");

        if (result.Command == Command.GetMainCategories)
        {
            var mainCategories = await _componentsService.GetMainCategoriesAsync();
            return Encoding.UTF8.GetBytes($"{{ \"command\": {(int)result.Command}, \"result\": {JsonSerializer.Serialize(mainCategories)} }}");
        }
        else if (result.Command == Command.GetBaseProperties)
        {
            var baseProperties = _componentsService.GetBaseProperties();
            return Encoding.UTF8.GetBytes($"{{ \"command\": {(int)result.Command}, \"result\": {JsonSerializer.Serialize(baseProperties)} }}");
        }
        else if (result.Command == Command.GetExtraProperties)
        {
            var extraProperties = await _componentsService.GetExtraPropertiesAsync(result.Data.MainCategory);
            return Encoding.UTF8.GetBytes($"{{ \"command\": {(int)result.Command}, \"result\": {JsonSerializer.Serialize(extraProperties).Replace("{", "").Replace("}", "")} }}");
        }
        else if (result.Command == Command.GetResults)
        {
            var results = await _componentsService.GetResultsAsync(result.Data.MainCategory, result.Data.ClientMap, result.Data.Query);

            if (results == null)
                return Encoding.UTF8.GetBytes($"{{ \"command\": {(int)result.Command}, \"result\": [] }}");
            return Encoding.UTF8.GetBytes($"{{ \"command\": {(int)result.Command}, \"result\": {results} }}");
        }
        else
        {
            return Encoding.UTF8.GetBytes("{ \"error\": 0 }");
        }
    }

    private async Task SocketLoop(WebSocket webSocket)
    {
        var buffer = new byte[1024];
        WebSocketReceiveResult receiveResult;

        do
        {
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (receiveResult.CloseStatus.HasValue)
                break;
            var responseBuffer = await Action(buffer, receiveResult.Count);
            await webSocket.SendAsync(
                new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);
        }
        while (true);

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}