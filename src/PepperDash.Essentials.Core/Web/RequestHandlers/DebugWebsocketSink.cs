using System;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

public class DebugWebsocketSink
{
    private bool _isRunning;
    private string _url;

    public bool IsRunning => _isRunning;
    public string Url => _url;

    public void StartServerAndSetPort(int port)
    {
        try
        {
            _url = $"ws://localhost:{port}";
            _isRunning = true;
            // Implement actual server startup logic here
        }
        catch (Exception ex)
        {
            _isRunning = false;
            throw new Exception($"Failed to start debug websocket server: {ex.Message}");
        }
    }

    public void StopServer()
    {
        try
        {
            // Implement actual server shutdown logic here
            _isRunning = false;
            _url = null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to stop debug websocket server: {ex.Message}");
        }
    }
}
