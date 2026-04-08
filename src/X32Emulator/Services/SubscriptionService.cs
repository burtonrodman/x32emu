using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using X32Emulator.Osc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace X32Emulator.Services;

public class SubscriptionService : BackgroundService
{
    private readonly ConcurrentDictionary<string, DateTime> _xremote = new();
    private readonly X32StateService _stateService;
    private readonly ILogger<SubscriptionService> _logger;
    private UdpClient? _udpClient;

    public SubscriptionService(X32StateService stateService, ILogger<SubscriptionService> logger)
    {
        _stateService = stateService;
        _logger = logger;
    }

    public void RegisterXRemote(IPEndPoint endpoint)
    {
        _xremote[endpoint.ToString()] = DateTime.UtcNow;
    }

    public void RegisterXRemoteFader(IPEndPoint endpoint)
    {
        _xremote[endpoint.ToString()] = DateTime.UtcNow;
    }

    public IEnumerable<IPEndPoint> XRemoteSubscribers
    {
        get
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-10);
            return _xremote
                .Where(kv => kv.Value >= cutoff)
                .Select(kv => IPEndPoint.Parse(kv.Key))
                .ToList();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _udpClient = new UdpClient();
        _stateService.OnStateChanged += BroadcastStateChange;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, stoppingToken);
                var cutoff = DateTime.UtcNow.AddSeconds(-10);
                var expired = _xremote.Where(kv => kv.Value < cutoff).Select(kv => kv.Key).ToList();
                foreach (var key in expired) _xremote.TryRemove(key, out _);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Error in SubscriptionService"); }
        }

        _stateService.OnStateChanged -= BroadcastStateChange;
        _udpClient?.Dispose();
    }

    private void BroadcastStateChange(string path, List<OscArg> args)
    {
        try
        {
            var msg = new OscMessage(path, args.ToArray());
            var bytes = OscSerializer.Serialize(msg);
            foreach (var ep in XRemoteSubscribers)
            {
                try { _udpClient?.Send(bytes, bytes.Length, ep); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to send to {Endpoint}", ep); }
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Error broadcasting state change"); }
    }
}
