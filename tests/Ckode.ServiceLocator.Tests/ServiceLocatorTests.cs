using System;
using System.Linq;
using Xunit;

namespace Ckode.Tests
{
    public class ServiceLocatorTests
    {
        public interface IStruct {}
        public struct StructImplementation: IStruct
        {
        }
        public interface IImplementation { }
        public interface IMultipleImplementations
        {
        }
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
            // Act
            var instance = ServiceLocator.CreateInstance<Implementation>();

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Implementation>(instance);
        }

        [Fact]
        public void CreateInstance_InterfaceHasOneImplementation_GivesInstance()
        {
            // Act
            var instance = ServiceLocator.CreateInstance<IImplementation>();

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<Implementation>(instance);
        }

        [Fact]
        public void CreateInstanceWithoutGenerics_InterfaceHasOneImplementation_GivesInstance()
        {
            // Act
            var instance = ServiceLocator.CreateInstance(typeof(IObjectImplementation));

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<ObjectImplementation>(instance);
        }

        [Fact]
        public void CreateInstance_InterfaceHasMultipleImplementations_Throws()
        {
            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<IMultipleImplementations>());
        }

        [Fact]
        public void CreateInstances_InterfaceHasMultipleImplementations_GivesInstances()
        {
            // Act
            var instances = ServiceLocator.CreateInstances<IMultipleImplementations>()
                                    .ToList();

            // Assert
            Assert.Equal(2, instances.Count);
        }

        [Fact]
        public void CreateInstance_ImplementationHasNoEmptyConstructor_Throws()
        {
            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<ImplementationWithoutEmptyConstructor>());
        }

        [Fact]
        public void CreateInstance_InterfaceHasNoImplementation_Throws()
        {
            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<IHasNoImplementation>());
        }

        [Fact]
        public void CreateInstanceWithBind_InterfaceHasMultipleImplementations_GivesInstance()
        {
            // Arrange
            ServiceLocator.Bind<IMultipleImplementations, ImplementationOne>();
            try
            {
                // Act
                var instance = ServiceLocator.CreateInstance<IMultipleImplementations>();

                // Assert
                Assert.IsType<ImplementationOne>(instance);
            }
            finally
            {
                ServiceLocator.Unbind<IMultipleImplementations>();
            }
        }

        [Fact]
        public void CreateInstanceWithBind_OverwriteBind_GivesProperInstance()
        {
            // Arrange
            ServiceLocator.Bind<IMultipleImplementations, ImplementationOne>();
            try
            {
                // Act
                var instance = ServiceLocator.CreateInstance<IMultipleImplementations>();

                // Assert
                Assert.IsType<ImplementationOne>(instance);

                // Rearrange
                ServiceLocator.Bind<IMultipleImplementations, ImplementationTwo>();

                // Act
                instance = ServiceLocator.CreateInstance<IMultipleImplementations>();

                // Assert
                Assert.IsType<ImplementationTwo>(instance);
            }
            finally
            {
                ServiceLocator.Unbind<IMultipleImplementations>();
            }
        }

        [Fact]
        public void CreateInstanceWithBindAndUnbind_InterfaceHasMultipleImplementations_Throws()
        {
            // Arrange
            ServiceLocator.Bind<IMultipleImplementations, ImplementationOne>();

            // Act
            var instance = ServiceLocator.CreateInstance<IMultipleImplementations>();

            // Assert
            Assert.IsType<ImplementationOne>(instance);


            // Rearrange
            ServiceLocator.Unbind<IMultipleImplementations>();

            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<IMultipleImplementations>());
        }

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInNoImplementations_Throws()
        {
            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<IHashingAlgorithm>(algo => algo.IsThisAlgorithm("sha256")));
        }

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInMultipleImplementations_Throws()
        {
            // Act && Assert
            Assert.Throws<ArgumentException>(() => ServiceLocator.CreateInstance<IHashingAlgorithm>(algo => true));
        }

        [Fact]
        public void CreateInstanceWithPredicate_PredicateResultsInSingleImplementation_GivesInstance()
        {
            // Act
            var algorithm = ServiceLocator.CreateInstance<IHashingAlgorithm>(algo => algo.IsThisAlgorithm("md5"));

            // Assert
            Assert.NotNull(algorithm);
            Assert.IsType<MD5>(algorithm);
        }
        
        [Fact]
        public void CreateInstance_ImplementationIsStruct_GivesInstance()
        {
            // Act
            var implementation = ServiceLocator.CreateInstance<IStruct>();
            
            // Assert
            Assert.IsType<StructImplementation>(implementation);
        }

        [Fact]
        public void CreateInstances_OnlyImplementationIsStruct_GivesInstance()
        {
            // Act
            var implementations = ServiceLocator.CreateInstances<IStruct>();
            
            // Assert
            var implementation = Assert.Single(implementations);
            Assert.IsType<StructImplementation>(implementation);
        }
    }
}
