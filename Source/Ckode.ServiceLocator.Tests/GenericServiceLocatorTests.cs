using System;
using System.Linq;
using Xunit;

namespace Ckode.ServiceLocator.Tests
{
    public class GenericServiceLocatorTests
    {
        public interface IImplementation { }
        public interface IMultipleImplementations { }
        public interface IHasNoImplementation { }

        public class Implementation : IImplementation { }
        public class ImplementationOne : IMultipleImplementations { }
        public class ImplementationTwo : IMultipleImplementations { }
        public class ImplementationWithoutEmptyConstructor
        {
            public ImplementationWithoutEmptyConstructor(int value) { }
        }


        [Fact]
        public void CreateInstance_UsingClassType_GivesInstance()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act
            var instance = locator.CreateInstance<Implementation>();

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Implementation>(instance);
        }

        [Fact]
        public void CreateInstance_InterfaceHasOneImplementation_GivesInstance()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act
            var instance = locator.CreateInstance<IImplementation>();

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Implementation>(instance);
        }

        [Fact]
        public void CreateInstance_InterfaceHasMultipleImplementations_Throws()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance<IMultipleImplementations>());
        }

        [Fact]
        public void CreateInstances_InterfaceHasMultipleImplementations_GivesInstances()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act
            var instances = locator.CreateInstances<IMultipleImplementations>()
                                    .ToList();

            // Assert
            Assert.Equal(2, instances.Count);
        }

        [Fact]
        public void CreateInstance_ImplementationHasNoEmptyConstructor_Throws()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance<ImplementationWithoutEmptyConstructor>());
        }

        [Fact]
        public void CreateInstance_InterfaceHasNoImplementation_Throws()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance<IHasNoImplementation>());
        }
    }
}
