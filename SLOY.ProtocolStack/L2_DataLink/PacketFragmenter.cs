using SLOY.Shared.Models;

namespace SLOY.ProtocolStack.L2_DataLink;

public class PacketFragmenter
{
    private readonly int _mtu;
    private readonly Dictionary<string, List<byte[]>> _fragmentBuffer = new();

    public PacketFragmenter(int mtu = 1024)
    {
        _mtu = mtu;
    }

    public List<Packet> Fragment(Packet packet)
    {
        if (packet.Size <= _mtu)
            return new List<Packet> { packet };

        var fragments = new List<Packet>();
        var data = packet.Payload;
        var totalFragments = (int)Math.Ceiling((double)data.Length / _mtu);
        var fragmentId = packet.Id[..8];

        for (int i = 0; i < totalFragments; i++)
        {
            var offset = i * _mtu;
            var length = Math.Min(_mtu, data.Length - offset);
            var fragmentData = new byte[length];
            Array.Copy(data, offset, fragmentData, 0, length);

            fragments.Add(new Packet
            {
                SenderId = packet.SenderId,
                ReceiverId = packet.ReceiverId,
                Protocol = packet.Protocol,
                Priority = packet.Priority,
                Payload = fragmentData,
                SequenceNumber = i
            });
        }

        return fragments;
    }

    public Packet? Reassemble(string fragmentGroupId, int sequenceNumber, int totalFragments, byte[] data)
    {
        if (!_fragmentBuffer.ContainsKey(fragmentGroupId))
            _fragmentBuffer[fragmentGroupId] = new List<byte[]>(new byte[totalFragments][]);

        var buffer = _fragmentBuffer[fragmentGroupId];
        if (buffer.Count <= sequenceNumber)
        {
            while (buffer.Count <= sequenceNumber)
                buffer.Add(Array.Empty<byte>());
        }

        buffer[sequenceNumber] = data;

        if (buffer.All(f => f.Length > 0) && buffer.Count == totalFragments)
        {
            var fullData = buffer.SelectMany(f => f).ToArray();
            _fragmentBuffer.Remove(fragmentGroupId);
            return new Packet { Payload = fullData };
        }

        return null;
    }
}