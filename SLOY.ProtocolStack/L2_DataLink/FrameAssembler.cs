namespace SLOY.ProtocolStack.L2_DataLink;

public class FrameAssembler
{
    private const byte StartByte = 0x7E;
    private const byte EscapeByte = 0x7D;
    private const byte EscapeXor = 0x20;

    private readonly List<byte> _buffer = new();
    private bool _inFrame;

    public event EventHandler<byte[]>? OnFrameReceived;

    public void ProcessByte(byte b)
    {
        if (b == StartByte)
        {
            if (_inFrame && _buffer.Count > 2)
            {
                OnFrameReceived?.Invoke(this, Unescape(_buffer.ToArray()));
            }
            _buffer.Clear();
            _inFrame = true;
            return;
        }

        if (_inFrame)
            _buffer.Add(b);
    }

    public byte[] CreateFrame(byte[] data, ushort crc = 0)
    {
        var escaped = Escape(data);
        var frame = new byte[escaped.Length + 4];

        frame[0] = StartByte;
        Array.Copy(escaped, 0, frame, 1, escaped.Length);

        crc = crc == 0 ? CalculateCrc16(data) : crc;
        frame[^3] = (byte)(crc >> 8);
        frame[^2] = (byte)(crc & 0xFF);
        frame[^1] = StartByte;

        return frame;
    }

    private byte[] Escape(byte[] data)
    {
        var result = new List<byte>();
        foreach (var b in data)
        {
            if (b == StartByte || b == EscapeByte)
            {
                result.Add(EscapeByte);
                result.Add((byte)(b ^ EscapeXor));
            }
            else
            {
                result.Add(b);
            }
        }
        return result.ToArray();
    }

    private byte[] Unescape(byte[] data)
    {
        var result = new List<byte>();
        var escape = false;

        foreach (var b in data)
        {
            if (escape)
            {
                result.Add((byte)(b ^ EscapeXor));
                escape = false;
            }
            else if (b == EscapeByte)
            {
                escape = true;
            }
            else
            {
                result.Add(b);
            }
        }

        return result.ToArray();
    }

    public static ushort CalculateCrc16(byte[] data)
    {
        ushort crc = 0xFFFF;
        foreach (var b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 1) == 1)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }
        return crc;
    }
}