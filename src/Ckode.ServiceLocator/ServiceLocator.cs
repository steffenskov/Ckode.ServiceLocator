using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ckode
{
    public sealed class ServiceLocator
        : BaseServiceLocator
    {
        private static readonly IDictionary<Type, Delegate> _constructors;
        private static readonly IDictionary<Type, IList<Delegate>> _multipleConstructors;
        private static readonly object _constructorsLock;
        private static readonly object _multipleConstructorsLock;
        private static readonly IDictionary<Type, Type> _boundImplementations;

        static ServiceLocator()
        {
            _constructorsLock = new object();
            _multipleConstructorsLock = new object();
            _constructors = new ConcurrentDictionary<Type, Delegate>();
            _multipleConstructors = new ConcurrentDictionary<Type, IList<Delegate>>();
            _boundImplementations = new ConcurrentDictionary<Type, Type>();
        }

        private ServiceLocator() // Don't support instantiation as everything is static
        {
        }

        /// <summary>
        /// Create an instance of the class that implements the given type.
        /// </summary>
        /// <typeparam name="T">The interface or baseclass the class must implement</typeparam>
        /// <returns>Instance of class</returns>
        public static T CreateInstance<T>()
        {
            var type = typeof(T);
            var constructorDelegate = _boundImplementations.TryGetValue(type, out var implementationType)
                                            ? GetConstructorDelegate(implementationType, CreateConstructorDelegate<T>)
                                            : GetConstructorDelegate(type, CreateConstructorDelegate<T>);

            return ((Func<T>)constructorDelegate)();
        }

        /// <summary>
        /// Create an instance of the class that implements the given type AND fulfills the predicate.
        /// </summary>
        /// <typeparam name="T">The interface or baseclass the class must implement</typeparam>
        /// <param name="predicate">Predicate which must be fulfilled for the instance to be returned</param>
        public static T CreateInstance<T>(Predicate<T> predicate)
        {
            var instances = CreateInstances<T>()
                                .Where(instance => predicate(instance))
                                .ToList();

            if (instances.Count == 0)
            {
                throw new ArgumentException($"No implementations of {typeof(T).Name} matched the given predicate.", nameof(predicate));
            }

            if (instances.Count > 1)
            {
                throw new ArgumentException($"Multiple implementations of {typeof(T).Name} matched the given predicate.", nameof(predicate));
            }

            return instances[0];
        }

        /// <summary>
        /// Create an instance of the class that implements the given type.
        /// </summary>
        /// <param name="type">The interface or baseclass the class must implement</param>
        /// <returns>Instance of class</returns>
        public static object CreateInstance(Type type)
        {
            var constructorDelegate = GetConstructorDelegate(type, CreateObjectConstructorDelegate);

            return ((Func<object>)constructorDelegate)();
        }

        private static Delegate GetConstructorDelegate(Type type, Func<Type, Delegate> createDelegate)
        {
            if (!_constructors.TryGetValue(type, out var constructorDelegate))
            {
                lock (_constructorsLock)
                {
                    if (!_constructors.TryGetValue(type, out constructorDelegate))
                    {
                        _constructors[type] = constructorDelegate = createDelegate(type);
                    }
                }
            }
            return constructorDelegate;
        }

        /// <summary>
        /// Create an instance of every class that implements the given interface.
        /// </summary>
        /// <typeparam name="T">The interface the classes must implement</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> CreateInstances<T>()
        {
            var interfaceType = typeof(T);
            if (!_multipleConstructors.TryGetValue(interfaceType, out var constructorDelegates))
            {
                lock (_multipleConstructorsLock)
                {
                    if (!_multipleConstructors.TryGetValue(interfaceType, out constructorDelegates))
                    {
                        _multipleConstructors[interfaceType] = constructorDelegates = CreateMultipleConstructorDelegates<T>(interfaceType);
                    }
                }
            }

            return constructorDelegates
                    .Cast<Func<T>>()
                    .Select(ctor => ctor());
        }

        public static void Bind<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            var interfaceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            _boundImplementations[interfaceType] = implementationType;
        }

        public static void Unbind<TInterface>()
        {
            var interfaceType = typeof(TInterface);
            _boundImplementations.Remove(interfaceType);
        }

        private static IList<Delegate> CreateMultipleConstructorDelegates<T>(Type interfaceType)
        {
            var implementationTypes = ImplementationTypes
                .Where(interfaceType.IsAssignableFrom)
                .ToList();

            var constructorInfos = implementationTypes
                                    .Where(type => type.IsClass)
                                    .Select(classType => classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null));

            var structDelegates = implementationTypes
                .Where(type => type.IsValueType)
                .Select(CreateStructDelegate<T>);

            return constructorInfos
                    .Select(CreateDelegate<T>)
                    .Concat(structDelegates)
                    .ToList();
        }

        private static Delegate CreateConstructorDelegate<T>(Type interfaceType)
        {
            var constructorInfo = GetConstructorInfo(interfaceType); // TODO: Struct support

            return CreateDelegate<T>(constructorInfo);
        }

        private static Delegate CreateObjectConstructorDelegate(Type interfaceType)
        {
            var constructorInfo = GetConstructorInfo(interfaceType); // TODO: Struct support

            return CreateDelegate(constructorInfo, interfaceType);
        }

        private static ConstructorInfo GetConstructorInfo(Type interfaceType)
        {
            IList<Type> implementationTypes = (interfaceType.IsInterface || interfaceType.IsAbstract)
                                        ? ImplementationTypes
                                            .Where(interfaceType.IsAssignableFrom)
                                            .ToArray()
                                        : new[] { interfaceType };

            if (implementationTypes.Count > 1)
            {
                throw new ArgumentException($"Multiple implementations of type {interfaceType.Name} exists, cannot create a single instance.", nameof(interfaceType));
            }
            if (implementationTypes.Count == 0)
            {
                throw new ArgumentException($"No implementations of type {interfaceType.Name} exists, cannot create an instance.", nameof(interfaceType));
            }
            var classType = implementationTypes[0];

            var constructorInfo = classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

            if (constructorInfo == null)
            {
                throw new ArgumentException($"The implementation of type {interfaceType.Name} doesn't have a parameterless constructor. This is required to create an instance.", nameof(interfaceType));
            }

            return constructorInfo;
        }
    }
}
