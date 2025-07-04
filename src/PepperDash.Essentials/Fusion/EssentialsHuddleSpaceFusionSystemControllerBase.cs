using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace PepperDash.Essentials.Fusion;

public class EssentialsHuddleSpaceFusionSystemControllerBase
{
    private ClientWebSocket _webSocket;

    public EssentialsHuddleSpaceFusionSystemControllerBase()
    {
        _webSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync(Uri uri)
    {
        await _webSocket.ConnectAsync(uri, CancellationToken.None);
    }

    public async Task SendAsync(string message)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<string> ReceiveAsync()
    {
        var buffer = new byte[1024];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    public async Task CloseAsync()
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
}
