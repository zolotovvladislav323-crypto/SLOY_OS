using System.Threading.Channels;

namespace SLOY.Core.Resilience;

public class BackpressureController<T>
{
    private readonly Channel<T> _channel;
    private readonly int _highWatermark;
    private readonly int _lowWatermark;

    public bool IsOverloaded => _channel.Reader.Count >= _highWatermark;

    public BackpressureController(int capacity = 1000, int highWatermark = 800, int lowWatermark = 200)
    {
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        _highWatermark = highWatermark;
        _lowWatermark = lowWatermark;
    }

    public async Task PushAsync(T item, CancellationToken ct = default)
    {
        if (IsOverloaded) await WaitForDrainAsync(ct);
        await _channel.Writer.WriteAsync(item, ct);
    }

    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);

    private async Task WaitForDrainAsync(CancellationToken ct)
    {
        while (_channel.Reader.Count > _lowWatermark)
            await Task.Delay(50, ct);
    }
}