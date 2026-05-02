namespace SLOY.Persistence.Secure.GarbageCollector;

public class AutoShredder
{
    private readonly string _targetPath;
    private readonly TimeSpan _interval;
    private CancellationTokenSource? _cts;

    public event EventHandler<string>? OnFileShredded;
    public event EventHandler<int>? OnBatchShredded;

    public AutoShredder(string targetPath, TimeSpan interval)
    {
        _targetPath = targetPath;
        _interval = interval;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        while (!_cts.Token.IsCancellationRequested)
        {
            await Task.Delay(_interval, _cts.Token);
            ShredExpiredFiles();
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private void ShredExpiredFiles()
    {
        if (!Directory.Exists(_targetPath)) return;

        var deletedCount = 0;
        var files = Directory.GetFiles(_targetPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (info.LastAccessTimeUtc + _interval < DateTime.UtcNow)
            {
                ShredFile(file);
                deletedCount++;
                OnFileShredded?.Invoke(this, file);
            }
        }

        if (deletedCount > 0)
            OnBatchShredded?.Invoke(this, deletedCount);
    }

    private void ShredFile(string path)
    {
        if (!File.Exists(path)) return;

        var info = new FileInfo(path);
        var size = info.Length;

        var random = new Random();
        var buffer = new byte[4096];

        for (int pass = 0; pass < 3; pass++)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Write);

            for (long written = 0; written < size; written += buffer.Length)
            {
                random.NextBytes(buffer);

                if (pass == 0)
                    Array.Fill(buffer, (byte)0x00);
                else if (pass == 1)
                    Array.Fill(buffer, (byte)0xFF);

                var toWrite = (int)Math.Min(buffer.Length, size - written);
                stream.Write(buffer, 0, toWrite);
                stream.Flush();
            }
        }

        File.Delete(path);
    }
}