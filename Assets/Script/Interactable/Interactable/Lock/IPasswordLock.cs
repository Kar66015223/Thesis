public interface IPasswordLock
{
    /// <summary>Returns true and unlocks when the code is correct. The code itself never leaves the lock.</summary>
    bool TrySubmit(string code);
}
