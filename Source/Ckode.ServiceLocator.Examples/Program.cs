using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ckode.ServiceLocator.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Variant1();
            Variant2();
            Variant3();
            Variant4();

        }

        static void Variant1()
        {
            var serviceLocator = new ServiceLocator();
            var userRepository = serviceLocator.CreateInstance<Variant1.IUserRepository>();
            Console.WriteLine("Variant 1:");
            Console.WriteLine($"Found user repository implementation: {userRepository.GetType().FullName}");
            Console.WriteLine();
        }

        static void Variant2()
        {
            var serviceLocator = new ServiceLocator();
            var repositories = serviceLocator.CreateInstances<Variant2.IRepository>();

            Console.WriteLine("Variant 2:");
            foreach (var repository in repositories)
            {
                Console.WriteLine($"Found a repository implementation: {repository.GetType().FullName}");
            }
            Console.WriteLine();
        }

        static void Variant3()
        {
            var serviceLocator = new ServiceLocator<Variant3.RepositoryType, Variant3.IRepository>();
            var userRepository = serviceLocator.CreateInstance(Examples.Variant3.RepositoryType.User);
            var companyRepository = serviceLocator.CreateInstance(Examples.Variant3.RepositoryType.Company);
            Console.WriteLine("Variant 3:");
            Console.WriteLine($"Found user repository implementation: {userRepository.GetType().FullName}");
            Console.WriteLine($"Found company repository implementation: {companyRepository.GetType().FullName}");
            Console.WriteLine();
        }

        static void Variant4()
        {
            var serviceLocator = new ServiceLocator<Variant4.RepositoryType, Variant4.IRepository>();
            var repositories = serviceLocator.CreateInstances();

            Console.WriteLine("Variant 4:");
            foreach (var repository in repositories)
            {
                Console.WriteLine($"Found a repository implementation: {repository.GetType().FullName} with the LocatorKey: {repository.LocatorKey.ToString()}");
            }
            Console.WriteLine();
        }
    }
}
