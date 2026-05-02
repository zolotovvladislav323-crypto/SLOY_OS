using SLOY.Core.Interfaces;
using SLOY.Shared.Models;

namespace SLOY.Orchestrator.Governance;

public class PeerDiscoveryBeacon
{
    private readonly IMeshRouter _router;
    private readonly Identity _identity;
    private CancellationTokenSource? _cts;
    private bool _isBroadcasting;

    public int BeaconIntervalMs { get; set; } = 5000;

    public PeerDiscoveryBeacon(IMeshRouter router, Identity identity)
    {
        _router = router;
        _identity = identity;
    }

    public async Task StartBroadcastingAsync()
    {
        _cts = new CancellationTokenSource();
        _isBroadcasting = true;

        while (!_cts.Token.IsCancellationRequested && _isBroadcasting)
        {
            var beacon = CreateBeacon();
            await _router.BroadcastAsync(beacon);
            await Task.Delay(BeaconIntervalMs, _cts.Token);
        }
    }

    public void StopBroadcasting()
    {
        _isBroadcasting = false;
        _cts?.Cancel();
    }

    private Packet CreateBeacon()
    {
        var payload = new List<byte>();

        payload.AddRange(System.Text.Encoding.UTF8.GetBytes(_identity.Nickname));
        payload.Add(0);
        payload.AddRange(System.Text.Encoding.UTF8.GetBytes(_identity.NodeId));
        payload.Add(0);
        payload.AddRange(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

        return new Packet
        {
            SenderId = _identity.NodeId,
            ReceiverId = "BROADCAST",
            Protocol = Shared.Enums.ProtocolType.Heartbeat,
            Priority = Shared.Enums.PacketPriority.Low,
            Payload = payload.ToArray()
        };
    }
}