using SLOY.Core.Interfaces;
using SLOY.Shared.Enums;
using SLOY.Shared.Models;

namespace SLOY.Orchestrator.Stealth;

public class DecoyTrafficGenerator
{
    private readonly IMeshRouter _router;
    private readonly Random _random = new();
    private CancellationTokenSource? _cts;
    private bool _isGenerating;

    public int PacketsSent { get; private set; }
    public int BytesWasted { get; private set; }

    public DecoyTrafficGenerator(IMeshRouter router)
    {
        _router = router;
    }

    public async Task StartAsync(int packetsPerMinute = 10)
    {
        _cts = new CancellationTokenSource();
        _isGenerating = true;
        var interval = 60000 / packetsPerMinute;

        while (!_cts.Token.IsCancellationRequested && _isGenerating)
        {
            await GenerateDecoyPacketAsync();
            await Task.Delay(interval + _random.Next(-interval / 3, interval / 3), _cts.Token);
        }
    }

    private async Task GenerateDecoyPacketAsync()
    {
        var decoySize = _random.Next(64, 1500);
        var decoyData = new byte[decoySize];
        _random.NextBytes(decoyData);

        var patterns = new[]
        {
            // Похоже на HTTP
            System.Text.Encoding.UTF8.GetBytes("GET / HTTP/1.1\r\nHost: cdn.example.com\r\n\r\n"),
            // Похоже на DNS
            new byte[] { 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
            // Похоже на TLS ClientHello
            new byte[] { 0x16, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00 },
            // Случайный мусор
            decoyData
        };

        var payload = patterns[_random.Next(patterns.Length)];

        var packet = new Packet
        {
            SenderId = Guid.NewGuid().ToString("N")[..12],
            ReceiverId = Guid.NewGuid().ToString("N")[..12],
            Protocol = ProtocolType.DecoyTraffic,
            Priority = PacketPriority.Low,
            Payload = payload,
            SequenceNumber = PacketsSent
        };

        await _router.BroadcastAsync(packet);

        PacketsSent++;
        BytesWasted += payload.Length;
    }

    public void Stop()
    {
        _isGenerating = false;
        _cts?.Cancel();
    }
}