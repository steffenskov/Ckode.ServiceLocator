using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ckode.ServiceLocator
{
    public sealed class ServiceLocator
        : BaseServiceLocator
    {
        private static readonly IDictionary<Type, Delegate> _constructors;
        private static readonly IDictionary<Type, IList<Delegate>> _multipleConstructors;
        private static readonly object _constructorsLock;
        private static readonly object _multipleConstructorsLock;

        static ServiceLocator()
        {
            _constructorsLock = new object();
            _multipleConstructorsLock = new object();
            _constructors = new ConcurrentDictionary<Type, Delegate>();
            _multipleConstructors = new ConcurrentDictionary<Type, IList<Delegate>>();
        }

        /// <summary>
        /// Create an instance of the class that implements the given type.
        /// </summary>
        /// <typeparam name="T">The interface or baseclass the class must implement</typeparam>
        /// <returns>Instance of class</returns>
        public T CreateInstance<T>()
        {
            var type = typeof(T);
            var constructorDelegate = GetConstructorDelegate(type, CreateConstructorDelegate<T>);

            return ((Func<T>)constructorDelegate)();
        }

        /// <summary>
        /// Create an instance of the class that implements the given type AND fulfills the predicate.
        /// </summary>
        /// <typeparam name="T">The interface or baseclass the class must implement</typeparam>
        /// <param name="predicate">Predicate which must be fulfilled for the instance to be returned</param>
        public T CreateInstance<T>(Predicate<T> predicate)
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
        public object CreateInstance(Type type)
        {
            var constructorDelegate = GetConstructorDelegate(type, CreateObjectConstructorDelegate);

            return ((Func<object>)constructorDelegate)();
        }

        private Delegate GetConstructorDelegate(Type type, Func<Type, Delegate> createDelegate)
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
        public IEnumerable<T> CreateInstances<T>()
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

        private IList<Delegate> CreateMultipleConstructorDelegates<T>(Type interfaceType)
        {
            var classTypes = ImplementationTypes
                                .Where(type => interfaceType.IsAssignableFrom(type));

            var constructorInfos = classTypes
                                    .Select(classType => classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null));

            return constructorInfos
                    .Select(CreateDelegate<T>)
                    .ToList();
        }

        private Delegate CreateConstructorDelegate<T>(Type interfaceType)
        {
            var constructorInfo = GetConstructorInfo(interfaceType);

            return CreateDelegate<T>(constructorInfo);
        }

        private Delegate CreateObjectConstructorDelegate(Type interfaceType)
        {
            var constructorInfo = GetConstructorInfo(interfaceType);

            return CreateDelegate(constructorInfo, interfaceType);
        }

        private static ConstructorInfo GetConstructorInfo(Type interfaceType)
        {
            IList<Type> classTypes = (interfaceType.IsInterface || interfaceType.IsAbstract)
                                        ? ImplementationTypes
                                            .Where(type => interfaceType.IsAssignableFrom(type))
                                            .ToArray()
                                        : new[] { interfaceType };

            if (classTypes.Count > 1)
            {
                throw new ArgumentException($"Multiple implementations of type {interfaceType.Name} exists, cannot create a single instance.", nameof(interfaceType));
            }
            if (classTypes.Count == 0)
            {
                throw new ArgumentException($"No implementations of type {interfaceType.Name} exists, cannot create an instance.", nameof(interfaceType));
            }
            var classType = classTypes[0];

            var constructorInfo = classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

            if (constructorInfo == null)
            {
                throw new ArgumentException($"The implementation of type {interfaceType.Name} doesn't have a parameterless constructor. This is required to create an instance.", nameof(interfaceType));
            }

            return constructorInfo;
        }
    }
}
