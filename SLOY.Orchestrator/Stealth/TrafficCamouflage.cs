using SLOY.Shared.Models;

namespace SLOY.Orchestrator.Stealth;

public class TrafficCamouflage
{
    private readonly Dictionary<CamouflageProfile, byte[]> _templates = new();
    private readonly Random _random = new();

    public TrafficCamouflage()
    {
        InitializeTemplates();
    }

    public byte[] Camouflage(Packet packet, CamouflageProfile profile)
    {
        return profile switch
        {
            CamouflageProfile.HTTP => WrapAsHttp(packet),
            CamouflageProfile.DNS => WrapAsDns(packet),
            CamouflageProfile.TLS => WrapAsTls(packet),
            CamouflageProfile.RTP => WrapAsRtp(packet),
            CamouflageProfile.ICMP => WrapAsIcmp(packet),
            _ => WrapAsRandom(packet)
        };
    }

    public Packet? Decamouflage(byte[] data)
    {
        var profile = DetectProfile(data);
        if (profile == CamouflageProfile.Unknown) return null;

        return profile switch
        {
            CamouflageProfile.HTTP => ExtractFromHttp(data),
            CamouflageProfile.DNS => ExtractFromDns(data),
            CamouflageProfile.TLS => ExtractFromTls(data),
            CamouflageProfile.RTP => ExtractFromRtp(data),
            _ => null
        };
    }

    private void InitializeTemplates()
    {
        _templates[CamouflageProfile.HTTP] = System.Text.Encoding.UTF8.GetBytes(
            "GET /api/v1/data HTTP/1.1\r\nHost: api.example.com\r\nUser-Agent: Mozilla/5.0\r\nAccept: application/json\r\n\r\n"
        );
        _templates[CamouflageProfile.DNS] = new byte[] { 0xAB, 0xCD, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        _templates[CamouflageProfile.TLS] = new byte[] { 0x16, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    }

    private byte[] WrapAsHttp(Packet packet)
    {
        var payload = Convert.ToBase64String(packet.Payload);
        var httpBody = System.Text.Encoding.UTF8.GetBytes(
            $"POST /upload HTTP/1.1\r\nContent-Type: application/octet-stream\r\nContent-Length: {payload.Length}\r\n\r\n{payload}"
        );
        return httpBody;
    }

    private byte[] WrapAsDns(Packet packet)
    {
        var result = new List<byte> { 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        result.AddRange(packet.Payload.Take(64));
        return result.ToArray();
    }

    private byte[] WrapAsTls(Packet packet)
    {
        var result = new List<byte> { 0x16, 0x03, 0x01 };
        var length = (ushort)Math.Min(packet.Payload.Length, 512);
        result.AddRange(BitConverter.GetBytes(length).Reverse());
        result.AddRange(packet.Payload.Take(length));
        return result.ToArray();
    }

    private byte[] WrapAsRtp(Packet packet)
    {
        var result = new List<byte> { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        result.AddRange(packet.Payload.Take(128));
        return result.ToArray();
    }

    private byte[] WrapAsIcmp(Packet packet)
    {
        var result = new List<byte> { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        result.AddRange(packet.Payload.Take(32));
        return result.ToArray();
    }

    private byte[] WrapAsRandom(Packet packet)
    {
        var result = new byte[packet.Payload.Length + 32];
        _random.NextBytes(result);
        Array.Copy(packet.Payload, 0, result, 16, Math.Min(packet.Payload.Length, result.Length - 16));
        return result;
    }

    private CamouflageProfile DetectProfile(byte[] data)
    {
        if (data.Length < 4) return CamouflageProfile.Unknown;

        if (data[0] == 0x50 || data[0] == 0x47)
            return CamouflageProfile.HTTP;
        if (data[0] == 0x00 && data[1] == 0x01)
            return CamouflageProfile.DNS;
        if (data[0] == 0x16 && data[1] == 0x03)
            return CamouflageProfile.TLS;
        if ((data[0] & 0xC0) == 0x80)
            return CamouflageProfile.RTP;

        return CamouflageProfile.Unknown;
    }

    private Packet? ExtractFromHttp(byte[] data)
    {
        var text = System.Text.Encoding.UTF8.GetString(data);
        var headerEnd = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (headerEnd < 0) return null;

        var body = text[(headerEnd + 4)..];
        try
        {
            var payload = Convert.FromBase64String(body.Trim());
            return new Packet { Payload = payload };
        }
        catch
        {
            return null;
        }
    }

    private Packet? ExtractFromDns(byte[] data)
    {
        if (data.Length <= 12) return null;
        return new Packet { Payload = data.Skip(12).ToArray() };
    }

    private Packet? ExtractFromTls(byte[] data)
    {
        if (data.Length <= 5) return null;
        var length = BitConverter.ToUInt16(new[] { data[4], data[3] });
        var payload = data.Skip(5).Take(length).ToArray();
        return new Packet { Payload = payload };
    }

    private Packet? ExtractFromRtp(byte[] data)
    {
        if (data.Length <= 12) return null;
        return new Packet { Payload = data.Skip(12).ToArray() };
    }
}

public enum CamouflageProfile
{
    Unknown,
    HTTP,
    DNS,
    TLS,
    RTP,
    ICMP
}