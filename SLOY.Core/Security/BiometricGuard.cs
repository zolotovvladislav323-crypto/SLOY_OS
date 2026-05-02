namespace SLOY.Core.Security;

public class BiometricGuard
{
    private int _failedAttempts;
    private readonly int _maxAttempts;
    private DateTime _lockoutUntil;
    private bool _isLocked;

    public bool IsLocked
    {
        get
        {
            if (_isLocked && DateTime.UtcNow > _lockoutUntil)
            {
                _isLocked = false;
                _failedAttempts = 0;
            }
            return _isLocked;
        }
    }

    public int RemainingAttempts => _isLocked ? 0 : _maxAttempts - _failedAttempts;
    public TimeSpan LockoutRemaining => _isLocked ? _lockoutUntil - DateTime.UtcNow : TimeSpan.Zero;

    public event EventHandler? OnLocked;
    public event EventHandler? OnUnlocked;
    public event EventHandler<int>? OnFailedAttempt;

    public BiometricGuard(int maxAttempts = 5)
    {
        _maxAttempts = maxAttempts;
    }

    public bool Validate(bool biometricSuccess)
    {
        if (IsLocked) return false;

        if (biometricSuccess)
        {
            _failedAttempts = 0;
            OnUnlocked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        _failedAttempts++;
        OnFailedAttempt?.Invoke(this, RemainingAttempts);

        if (_failedAttempts >= _maxAttempts)
        {
            _isLocked = true;
            _lockoutUntil = DateTime.UtcNow.AddMinutes(Math.Pow(2, _failedAttempts - _maxAttempts));
            OnLocked?.Invoke(this, EventArgs.Empty);
        }

        return false;
    }
}