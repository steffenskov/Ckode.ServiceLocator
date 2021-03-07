using System;
using System.Linq;
using Xunit;

namespace Ckode.ServiceLocator.Tests
{
    public class ServiceLocatorTests
    {
        public interface IImplementation { }
        public interface IMultipleImplementations { }
        public interface IHasNoImplementation { }
        public interface IObjectImplementation { }
        public interface IHashingAlgorithm
        {
            bool IsThisAlgorithm(string hashedValue);
        }

        public class ObjectImplementation : IObjectImplementation { }
        public class Implementation : IImplementation { }
        public class ImplementationOne : IMultipleImplementations { }
        public class ImplementationTwo : IMultipleImplementations { }
        public class ImplementationWithoutEmptyConstructor
        {
            public ImplementationWithoutEmptyConstructor(int value) { }
        }

        public class MD5 : IHashingAlgorithm
        {
            public bool IsThisAlgorithm(string hashedValue)
            {
                return hashedValue?.StartsWith("md5", StringComparison.OrdinalIgnoreCase) == true; // simple approach for unit test
            }
        }
        public class SHA1 : IHashingAlgorithm
        {
            public bool IsThisAlgorithm(string hashedValue)
            {
                return hashedValue?.StartsWith("sha1", StringComparison.OrdinalIgnoreCase) == true; // simple approach for unit test
            }
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
        public void CreateInstanceWithoutGenerics_InterfaceHasOneImplementation_GivesInstance()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act
            var instance = locator.CreateInstance(typeof(IObjectImplementation));

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<ObjectImplementation>(instance);
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

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInNoImplementations_Throws()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance<IHashingAlgorithm>(algo => algo.IsThisAlgorithm("sha256")));
        }

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInMultipleImplementations_Throws()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => locator.CreateInstance<IHashingAlgorithm>(algo => true));
        }

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInSingleImplementation_GivesInstance()
        {
            // Arrange
            var locator = new ServiceLocator();

            // Act
            var algorithm = locator.CreateInstance<IHashingAlgorithm>(algo => algo.IsThisAlgorithm("md5"));

            // Assert
            Assert.NotNull(algorithm);
            Assert.IsType<MD5>(algorithm);
        }
    }
}
