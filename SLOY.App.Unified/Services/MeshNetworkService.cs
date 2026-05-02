using SLOY.Core.Interfaces;
using SLOY.Orchestrator;
using SLOY.Orchestrator.Governance;
using SLOY.Orchestrator.Stealth;
using SLOY.Shared.Models;

namespace SLOY.App.Unified.Services;

public class MeshNetworkService
{
    private readonly IMeshRouter _router;
    private readonly MeshLifecycleManager _lifecycle;
    private readonly PeerDiscoveryBeacon _beacon;
    private readonly DecoyTrafficGenerator _decoyGenerator;
    private readonly TrafficCamouflage _camouflage;
    private bool _isRunning;

    public MeshLifecycleManager Lifecycle => _lifecycle;
    public bool IsRunning => _isRunning;

    public MeshNetworkService(IMeshRouter router, Identity identity)
    {
        _router = router;
        _lifecycle = new MeshLifecycleManager(router);
        _beacon = new PeerDiscoveryBeacon(router, identity);
        _decoyGenerator = new DecoyTrafficGenerator(router);
        _camouflage = new TrafficCamouflage();
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        await _lifecycle.StartAsync();
        await _beacon.StartBroadcastingAsync();
        await _decoyGenerator.StartAsync(packetsPerMinute: 10);
    }

    public void Stop()
    {
        _isRunning = false;
        _lifecycle.StopAsync();
        _beacon.StopBroadcasting();
        _decoyGenerator.Stop();
    }

    public async Task SendMessageAsync(string receiverId, string message)
    {
        var packet = new Packet
        {
            SenderId = App.CurrentIdentity?.NodeId ?? "unknown",
            ReceiverId = receiverId,
            Protocol = Shared.Enums.ProtocolType.TextMessage,
            Priority = Shared.Enums.PacketPriority.Normal,
            Payload = System.Text.Encoding.UTF8.GetBytes(message)
        };

        var camouflaged = _camouflage.Camouflage(packet, CamouflageProfile.HTTP);
        packet.Payload = camouflaged;

        await _router.SendAsync(packet);
    }
}