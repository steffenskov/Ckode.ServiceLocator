namespace Ckode
{
    public interface ILocatable<TKey>
    {
        TKey LocatorKey { get; }
    }
}