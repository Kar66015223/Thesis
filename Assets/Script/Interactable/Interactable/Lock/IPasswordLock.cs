public interface IPasswordLock
{
    bool TrySubmit(string code);
}