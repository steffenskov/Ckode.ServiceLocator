namespace Ckode.ServiceLocator
{
    public interface ILocatable<TKey>
    {
        TKey LocatorKey { get; }
    }
}