namespace SLOY.Core.Resilience;

public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly Func<int, TimeSpan> _delayProvider;

    public RetryPolicy(int maxRetries = 3, string strategy = "exponential")
    {
        _maxRetries = maxRetries;
        _delayProvider = strategy switch
        {
            "fixed" => _ => TimeSpan.FromMilliseconds(500),
            "exponential" => i => TimeSpan.FromMilliseconds(Math.Pow(2, i) * 100),
            _ => i => TimeSpan.FromMilliseconds(Math.Pow(2, i) * 100)
        };
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        for (int i = 0; i <= _maxRetries; i++)
        {
            try { return await action(); }
            catch when (i < _maxRetries) { await Task.Delay(_delayProvider(i), ct); }
        }
        throw new InvalidOperationException("Все попытки исчерпаны.");
    }
}