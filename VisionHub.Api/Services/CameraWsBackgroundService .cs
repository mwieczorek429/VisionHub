using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using VisionHub.Api.Models.Camera;


namespace VisionHub.Api.Services;

public sealed class CameraWsBackgroundService : BackgroundService
{
    private readonly ILogger<CameraWsBackgroundService> _log;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly Dictionary<int, ClientWebSocket> _active = new();

    public CameraWsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CameraWsBackgroundService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await OpenMissingSocketsAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task OpenMissingSocketsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<ICameraRepository>();
        var cameraService = scope.ServiceProvider.GetRequiredService<CameraService>();

        var cameras = repo.Cameras.ToList();

        foreach (var cam in cameras)
        {
            if (_active.ContainsKey(cam.Id)) continue;

            var wsUrl = cam.Url.Replace("http", "ws").TrimEnd('/') + "/camera/ws";
            var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", $"Bearer {cam.Token}");

            try
            {
                await socket.ConnectAsync(new Uri(wsUrl), ct);
                _active[cam.Id] = socket;
                _log.LogInformation("WS open for camera {Id}", cam.Id);

                _ = Task.Run(() => ListenLoopAsync(cam.Id, socket, ct), ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to open WS for camera {Id}, attempting to update token", cam.Id);

                try
                {
                    await cameraService.UpdateTokenAsync(cam);
                    _log.LogInformation("Token updated for camera {Id}", cam.Id);
                }
                catch (Exception updateEx)
                {
                    _log.LogError(updateEx, "Failed to update token for camera {Id}", cam.Id);
                }

                socket.Dispose();
            }
        }
    }
    private async Task ListenLoopAsync(int cameraId, ClientWebSocket socket, CancellationToken ct)
    {
        var buf = new byte[4096];

        using var scope = _scopeFactory.CreateScope();
        var eventRepo = scope.ServiceProvider.GetRequiredService<ICameraEventRepository>();

        while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var res = await socket.ReceiveAsync(buf, ct);
            if (res.MessageType == WebSocketMessageType.Close) break;

            var json = Encoding.UTF8.GetString(buf, 0, res.Count);
            try
            {
                var statusDto = JsonSerializer.Deserialize<CameraStatusDto>(json);

                if (statusDto != null)
                {
                    var cameraEvent = new CameraEvent
                    {
                        CameraId = cameraId,
                        Timestamp = statusDto.Timestamp,
                        MotionDetected = statusDto.MotionDetected,
                        Object = statusDto.Object
                    };

                    await eventRepo.AddCameraEventAsync(cameraEvent); 
                }
            }
            catch (JsonException)
            {
                _log.LogWarning("Bad JSON from cam {Id}", cameraId);
            }
        }

        socket.Dispose();
        _active.Remove(cameraId);
        _log.LogWarning("WS closed for camera {Id}", cameraId);
    }


    public override async Task StopAsync(CancellationToken ct)
    {
        foreach (var s in _active.Values)
            if (s.State == WebSocketState.Open)
                await s.CloseAsync(WebSocketCloseStatus.NormalClosure, "Shutdown", ct);
        await base.StopAsync(ct);
    }
}