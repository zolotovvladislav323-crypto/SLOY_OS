using SLOY.Shared.Models;

namespace SLOY.ProtocolStack.L4_Transport;

public class ReliableDelivery
{
    private readonly Dictionary<string, PendingPacket> _pendingAcks = new();
    private readonly int _timeoutMs;
    private readonly int _maxRetries;

    public event EventHandler<Packet>? OnAcknowledged;
    public event EventHandler<Packet>? OnFailed;

    public ReliableDelivery(int timeoutMs = 3000, int maxRetries = 5)
    {
        _timeoutMs = timeoutMs;
        _maxRetries = maxRetries;
    }

    public void SendPacket(Packet packet, Func<Packet, Task> sendFunc)
    {
        var pending = new PendingPacket
        {
            Packet = packet,
            RetryCount = 0,
            SendFunc = sendFunc,
            SentAt = DateTime.UtcNow
        };

        _pendingAcks[packet.Id] = pending;
        _ = MonitorAckAsync(packet.Id);
    }

    public void ReceiveAck(string packetId)
    {
        if (_pendingAcks.TryGetValue(packetId, out var pending))
        {
            _pendingAcks.Remove(packetId);
            OnAcknowledged?.Invoke(this, pending.Packet);
        }
    }

    private async Task MonitorAckAsync(string packetId)
    {
        while (_pendingAcks.TryGetValue(packetId, out var pending))
        {
            if (pending.RetryCount >= _maxRetries)
            {
                _pendingAcks.Remove(packetId);
                OnFailed?.Invoke(this, pending.Packet);
                return;
            }

            var elapsed = (DateTime.UtcNow - pending.SentAt).TotalMilliseconds;
            if (elapsed > _timeoutMs * Math.Pow(2, pending.RetryCount))
            {
                pending.RetryCount++;
                pending.SentAt = DateTime.UtcNow;
                await pending.SendFunc(pending.Packet);
            }

            await Task.Delay(100);
        }
    }

    public int GetPendingCount() => _pendingAcks.Count;

    private class PendingPacket
    {
        public Packet Packet { get; set; } = null!;
        public int RetryCount { get; set; }
        public Func<Packet, Task> SendFunc { get; set; } = null!;
        public DateTime SentAt { get; set; }
    }
}