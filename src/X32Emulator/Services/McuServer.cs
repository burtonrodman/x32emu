using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using X32Emulator.Osc;

namespace X32Emulator.Services;

public class McuServer : BackgroundService
{
    private readonly X32StateService _stateService;
    private readonly ILogger<McuServer> _logger;
    private readonly ConcurrentDictionary<Guid, TcpClient> _clients = new();

    private static readonly byte[] DeviceInquiryResponse = new byte[]
    {
        0xF0, 0x7E, 0x7F, 0x06, 0x02, 0x00, 0x00, 0x66, 0x14, 0x00, 0x00, 0x00, 0x04, 0x06, 0xF7
    };

    public McuServer(X32StateService stateService, ILogger<McuServer> logger)
    {
        _stateService = stateService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Any, 10111);
        listener.Start();
        _logger.LogInformation("MCU server listening on TCP port 10111");

        _stateService.OnStateChanged += OnStateChanged;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                var id = Guid.NewGuid();
                _clients[id] = client;
                _ = HandleClientAsync(id, client, stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _stateService.OnStateChanged -= OnStateChanged;
            listener.Stop();
            foreach (var c in _clients.Values) c.Dispose();
        }
    }

    private async Task HandleClientAsync(Guid id, TcpClient client, CancellationToken ct)
    {
        try
        {
            var stream = client.GetStream();
            await stream.WriteAsync(DeviceInquiryResponse, ct);

            var buffer = new byte[3];
            while (!ct.IsCancellationRequested && client.Connected)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(0, 1), ct);
                if (read == 0) break;

                var status = buffer[0];

                if (status >= 0xE0 && status <= 0xE7)
                {
                    int channel = status & 0x0F;
                    var data = new byte[2];
                    int r = await ReadExactAsync(stream, data, 2, ct);
                    if (r < 2) break;
                    int lsb = data[0] & 0x7F;
                    int msb = data[1] & 0x7F;
                    int value14 = (msb << 7) | lsb;
                    float faderVal = value14 / 16383.0f;
                    if (channel < 8)
                    {
                        var chPath = $"/ch/{channel + 1:D2}/mix/fader";
                        _stateService.SetValue(chPath, new List<OscArg> { new OscArg(faderVal) });
                    }
                }
                else if (status == 0x90)
                {
                    var data = new byte[2];
                    int r = await ReadExactAsync(stream, data, 2, ct);
                    if (r < 2) break;
                    int note = data[0];
                    if (note >= 0x18 && note <= 0x1F)
                    {
                        int chIdx = note - 0x18;
                        var chPath = $"/ch/{chIdx + 1:D2}/mix/on";
                        var current = _stateService.GetValue(chPath);
                        bool currentOn = current?.AsInt() != 0;
                        _stateService.SetValue(chPath, new List<OscArg> { new OscArg(currentOn ? 0 : 1) });
                    }
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "MCU client disconnected");
        }
        finally
        {
            _clients.TryRemove(id, out _);
            client.Dispose();
        }
    }

    private async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int count, CancellationToken ct)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(totalRead, count - totalRead), ct);
            if (read == 0) return totalRead;
            totalRead += read;
        }
        return totalRead;
    }

    private void OnStateChanged(string path, List<OscArg> args)
    {
        if (args.Count == 0) return;

        byte[]? midiBytes = null;

        if (path.StartsWith("/ch/") && path.EndsWith("/mix/fader"))
        {
            var parts = path.Split('/');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int chNum) && chNum >= 1 && chNum <= 8)
            {
                float faderVal = args[0].AsFloat();
                int value14 = (int)(faderVal * 16383);
                int lsb = value14 & 0x7F;
                int msb = (value14 >> 7) & 0x7F;
                byte channel = (byte)(0xE0 | (chNum - 1));
                midiBytes = new byte[] { channel, (byte)lsb, (byte)msb };
            }
        }
        else if (path.StartsWith("/ch/") && path.EndsWith("/mix/on"))
        {
            var parts = path.Split('/');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int chNum) && chNum >= 1 && chNum <= 8)
            {
                bool on = args[0].AsInt() != 0;
                byte note = (byte)(0x18 + (chNum - 1));
                byte velocity = on ? (byte)0x00 : (byte)0x7F;
                midiBytes = new byte[] { 0x90, note, velocity };
            }
        }

        if (midiBytes != null)
        {
            foreach (var client in _clients.Values)
            {
                try
                {
                    if (client.Connected)
                        client.GetStream().Write(midiBytes, 0, midiBytes.Length);
                }
                catch { }
            }
        }
    }
}
