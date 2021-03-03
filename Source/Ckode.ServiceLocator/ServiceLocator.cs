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
        /// Create an instance of the class that implements the given interface.
        /// </summary>
        /// <typeparam name="T">The interface the class must implement</typeparam>
        /// <returns></returns>
        public T CreateInstance<T>()
        {
            var interfaceType = typeof(T);
            if (!_constructors.TryGetValue(interfaceType, out var ctorDelegate))
            {
                lock (_constructorsLock)
                {
                    if (!_constructors.TryGetValue(interfaceType, out ctorDelegate))
                    {
                        _constructors[interfaceType] = ctorDelegate = CreateCtorDelegate<T>(interfaceType);
                    }
                }
            }

            return ((Func<T>)ctorDelegate)();
        }

        public object CreateInstance(Type type)
        {
            if (!_constructors.TryGetValue(type, out var ctorDelegate))
            {
                lock (_constructorsLock)
                {
                    if (!_constructors.TryGetValue(type, out ctorDelegate))
                    {
                        _constructors[type] = ctorDelegate = CreateObjectCtorDelegate(type);
                    }
                }
            }

            return ((Func<object>)ctorDelegate)();
        }

        /// <summary>
        /// Create an instance of every class that implements the given interface.
        /// </summary>
        /// <typeparam name="T">The interface the classes must implement</typeparam>
        /// <returns></returns>
        public IEnumerable<T> CreateInstances<T>()
        {
            var interfaceType = typeof(T);
            if (!_multipleConstructors.TryGetValue(interfaceType, out var ctorDelegates))
            {
                lock (_multipleConstructorsLock)
                {
                    if (!_multipleConstructors.TryGetValue(interfaceType, out ctorDelegates))
                    {
                        _multipleConstructors[interfaceType] = ctorDelegates = CreateMultipleCtorDelegates<T>(interfaceType);
                    }
                }
            }

            return ctorDelegates
                .Cast<Func<T>>()
                .Select(ctor => ctor());
        }

        private IList<Delegate> CreateMultipleCtorDelegates<T>(Type interfaceType)
        {
            var classTypes = Types.Where(type => interfaceType.IsAssignableFrom(type));

            var ctorInfos = classTypes.Select(classType => classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null));

            return ctorInfos
                .Select(ctorInfo => CreateDelegate<T>(ctorInfo))
                .ToArray();
        }

        private Delegate CreateCtorDelegate<T>(Type interfaceType)
        {
            var ctorInfo = GetConstructorInfo(interfaceType);

            return CreateDelegate<T>(ctorInfo);
        }

        private Delegate CreateObjectCtorDelegate(Type interfaceType)
        {
            var ctorInfo = GetConstructorInfo(interfaceType);

            return CreateDelegate(ctorInfo, interfaceType);
        }

        private static ConstructorInfo GetConstructorInfo(Type interfaceType)
        {
            IList<Type> classTypes = (interfaceType.IsInterface || interfaceType.IsAbstract)
                                        ? Types
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

            var ctorInfo = classType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

            if (ctorInfo == null)
            {
                throw new ArgumentException($"The implementation of type {interfaceType.Name} doesn't have a parameterless constructor. This is required to create an instance.", nameof(interfaceType));
            }

            return ctorInfo;
        }
    }
}
