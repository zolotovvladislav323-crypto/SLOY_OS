using System.Collections.Concurrent;

namespace SLOY.Core.Resilience;

public class RateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly int _maxTokens;
    private readonly double _refillRate;

    public RateLimiter(int maxTokens = 100, double refillPerSecond = 10)
    {
        _maxTokens = maxTokens;
        _refillRate = refillPerSecond;
    }

    public bool TryConsume(string key, int tokens = 1)
    {
        var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_maxTokens, _refillRate));
        return bucket.TryConsume(tokens);
    }

    private class TokenBucket
    {
        private double _tokens;
        private DateTime _lastRefill = DateTime.UtcNow;
        private readonly int _max;
        private readonly double _rate;

        public TokenBucket(int max, double rate) { _max = max; _rate = rate; _tokens = max; }

        public bool TryConsume(int tokens)
        {
            Refill();
            if (_tokens < tokens) return false;
            _tokens -= tokens;
            return true;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastRefill).TotalSeconds;
            _tokens = Math.Min(_max, _tokens + elapsed * _rate);
            _lastRefill = now;
        }
    }
}