namespace Ckode.ServiceLocator.Examples.Variant4
{
    class CustomerRepository : IRepository
    {
        public RepositoryType LocatorKey => RepositoryType.Customer;
    }
}
