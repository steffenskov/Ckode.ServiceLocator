using System;
using System.Linq;
using Xunit;

namespace Ckode.ServiceLocator.Tests
{
    public class GenericServiceLocatorTests
    {
        public enum LocatorKey
        {
            Unimplemented,
            Implemented
        }

        public interface IPartlyImplemented : ILocatable<LocatorKey> { }

        public class Implemented : IPartlyImplemented
        {
            public LocatorKey LocatorKey => LocatorKey.Implemented;
        }

        public interface ICompletelyUnImplemented : ILocatable<LocatorKey> { }

        public interface IDoubleImplementedKey : ILocatable<LocatorKey> { }

        public class FirstDoubleImplementation : IDoubleImplementedKey
        {
            public LocatorKey LocatorKey => LocatorKey.Implemented;
        }

        public class SecondDoubleImplementation : IDoubleImplementedKey
        {
            public LocatorKey LocatorKey => LocatorKey.Implemented;
        }

        [Fact]
        public void CreateInstance_UsingUnimplementedKey_Throws()
        {
            // Arrange
            var locator = new ServiceLocator<LocatorKey, IPartlyImplemented>();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance(LocatorKey.Unimplemented));
        }

        [Fact]
        public void CreateInstance_UsingValidKey_GivesInstance()
        {
            // Arrange
            var locator = new ServiceLocator<LocatorKey, IPartlyImplemented>();

            // Act
            var instance = locator.CreateInstance(LocatorKey.Implemented);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Implemented>(instance);
        }

        [Fact]
        public void CreateInstances_HasSomeImplementations_GivesInstances()
        {
            // Arrange
            var locator = new ServiceLocator<LocatorKey, IPartlyImplemented>();

            // Act
            var instances = locator.CreateInstances();

            // Assert
            Assert.NotNull(instances);
            Assert.Single(instances);
        }

        [Fact]
        public void InstantiateServiceLocator_InterfaceHasNoImplementations_Throws()
        {
            // Arrange, Act && Assert
            Assert.Throws<ArgumentException>(() => new ServiceLocator<LocatorKey, ICompletelyUnImplemented>());
        }

        [Fact]
        public void InstantiateServiceLocator_InterfaceMultipleImplementationsWithSameKey_Throws()
        {
            // Arrange, Act && Assert
            Assert.Throws<ArgumentException>(() => new ServiceLocator<LocatorKey, IDoubleImplementedKey>());
        }
    }
}
