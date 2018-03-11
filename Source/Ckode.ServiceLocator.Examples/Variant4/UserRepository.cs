namespace Ckode.ServiceLocator.Examples.Variant4
{
    class UserRepository : IRepository
    {
        public RepositoryType LocatorKey => RepositoryType.User;
    }
}
