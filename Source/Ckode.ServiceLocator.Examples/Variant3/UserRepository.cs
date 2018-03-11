namespace Ckode.ServiceLocator.Examples.Variant3
{
    class UserRepository : IRepository
    {
        public RepositoryType LocatorKey => RepositoryType.User;
    }
}
