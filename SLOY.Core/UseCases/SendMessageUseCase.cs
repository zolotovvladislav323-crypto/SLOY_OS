using SLOY.Core.Interfaces;
using SLOY.Core.Pipeline;
using SLOY.Core.Repository;
using SLOY.Shared.Models;

namespace SLOY.Core.UseCases;

public class SendMessageUseCase
{
    private readonly IMeshRouter _router;
    private readonly IMessageRepository _messages;
    private readonly IPacketRepository _packets;
    private readonly PacketPipeline _pipeline;

    public SendMessageUseCase(IMeshRouter router, IMessageRepository messages, IPacketRepository packets, PacketPipeline pipeline)
    {
        _router = router; _messages = messages; _packets = packets; _pipeline = pipeline;
    }

    public async Task<bool> ExecuteAsync(Message message)
    {
        await _messages.SaveAsync(message);
        var packet = new Packet
        {
            SenderId = message.SenderNodeId,
            ReceiverId = message.SenderNodeId,
            Payload = System.Text.Encoding.UTF8.GetBytes(message.Content)
        };
        var processed = await _pipeline.ExecuteAsync(packet);
        await _packets.SaveAsync(processed);
        await _router.SendAsync(processed);
        return true;
    }
}