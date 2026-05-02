using System.IO.Compression;
using SLOY.Shared.Models;

namespace SLOY.Core.Pipeline;

public class CompressionMiddleware : IPacketMiddleware
{
    public async Task<Packet> ProcessAsync(Packet packet, Func<Packet, Task<Packet>> next)
    {
        using var outStream = new MemoryStream();
        using (var gzip = new GZipStream(outStream, CompressionLevel.Optimal))
            gzip.Write(packet.Payload);
        packet.Payload = outStream.ToArray();
        return await next(packet);
    }
}