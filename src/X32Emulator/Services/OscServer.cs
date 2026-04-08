using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using X32Emulator.Osc;

namespace X32Emulator.Services;

public class OscServer : BackgroundService
{
    private readonly X32StateService _stateService;
    private readonly SubscriptionService _subscriptionService;
    private readonly SceneService _sceneService;
    private readonly ILogger<OscServer> _logger;

    public OscServer(X32StateService stateService, SubscriptionService subscriptionService, SceneService sceneService, ILogger<OscServer> logger)
    {
        _stateService = stateService;
        _subscriptionService = subscriptionService;
        _sceneService = sceneService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var udp = new UdpClient(10023);
        _logger.LogInformation("OSC server listening on UDP port 10023");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await udp.ReceiveAsync(stoppingToken);
                var msg = OscSerializer.Deserialize(result.Buffer);
                if (msg == null) continue;

                var sender = result.RemoteEndPoint;
                var senderIp = sender.Address.ToString();

                try
                {
                    await HandleMessage(udp, msg, sender, senderIp, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OSC message {Address}", msg.Address);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Error receiving OSC message"); }
        }
    }

    private async Task HandleMessage(UdpClient udp, OscMessage msg, IPEndPoint sender, string senderIp, CancellationToken ct)
    {
        switch (msg.Address)
        {
            case "/xinfo":
                var infoReply = new OscMessage("/xinfo",
                    new OscArg(senderIp),
                    new OscArg(_stateService.State.Config.Name),
                    new OscArg("X32RACK"),
                    new OscArg("4.06"));
                await SendReply(udp, infoReply, sender);
                break;

            case "/status":
                var statusReply = new OscMessage("/status",
                    new OscArg("active"),
                    new OscArg(senderIp),
                    new OscArg("X32RACK"));
                await SendReply(udp, statusReply, sender);
                break;

            case "/xremote":
                _subscriptionService.RegisterXRemote(sender);
                break;

            case "/xremotefader":
                _subscriptionService.RegisterXRemoteFader(sender);
                break;

            case "/node":
                if (msg.Arguments.Count > 0)
                {
                    var nodePath = msg.Arguments[0].AsString();
                    var children = _stateService.GetNodeChildren(nodePath);
                    var nodeReply = new OscMessage("/node", new OscArg(children));
                    await SendReply(udp, nodeReply, sender);
                }
                break;

            default:
                if (msg.Address.StartsWith("/-action/goscene"))
                {
                    if (msg.Arguments.Count > 0)
                        _sceneService.GoToScene(msg.Arguments[0].AsInt());
                }
                else if (msg.Address.StartsWith("/meters/"))
                {
                    // stub - no-op
                }
                else if (msg.Arguments.Count == 0)
                {
                    var val = _stateService.GetValue(msg.Address);
                    if (val != null)
                    {
                        var reply = new OscMessage(msg.Address, val);
                        await SendReply(udp, reply, sender);
                    }
                }
                else
                {
                    _stateService.SetValue(msg.Address, msg.Arguments);
                }
                break;
        }
    }

    private async Task SendReply(UdpClient udp, OscMessage msg, IPEndPoint endpoint)
    {
        var bytes = OscSerializer.Serialize(msg);
        await udp.SendAsync(bytes, bytes.Length, endpoint);
    }
}
