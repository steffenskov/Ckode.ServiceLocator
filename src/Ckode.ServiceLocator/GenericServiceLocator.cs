using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ckode
{
    public class ServiceLocator<TKey, T>
          : BaseServiceLocator
          where T : ILocatable<TKey>
    {
        private readonly IDictionary<TKey, Func<T>> _constructors;
        private static readonly ConcurrentDictionary<Type, IDictionary<TKey, Func<T>>> _cachedConstructors;
        private static readonly object _cacheLock;

        static ServiceLocator()
        {
            _cacheLock = new object();
            _cachedConstructors = new ConcurrentDictionary<Type, IDictionary<TKey, Func<T>>>();
        }

        public ServiceLocator()
        {
            var locatorType = GetType();
            if (_cachedConstructors.TryGetValue(locatorType, out _constructors))
            {
                return;
            }

            lock (_cacheLock)
            {
                if (_cachedConstructors.TryGetValue(locatorType, out _constructors))
                {
                    return;
                }

                _constructors = new Dictionary<TKey, Func<T>>();
                var interfaceType = typeof(T);

                var foundImplementations = ImplementationTypes.Where(type => interfaceType.IsAssignableFrom(type)).ToList();
                if (!foundImplementations.Any())
                {
                    throw new ArgumentException($"The type {interfaceType.Name} has no implementations.");
                }

                foreach (var implementationType in foundImplementations)
                {
                    Func<T> func;
                    if (implementationType.IsValueType)
                        func = (Func<T>)CreateStructDelegate<T>(implementationType); 
                    else 
                        func= CreateConstructorDelegate(implementationType);

                    var instance = func();
                    var key = instance.LocatorKey;
                    if (_constructors.ContainsKey(key))
                    {
                        throw new ArgumentException($"Multiple classes are not allowed to return the same LocatorKey: {key}");
                    }

                    _constructors[key] = func;
                }
                _cachedConstructors[locatorType] = _constructors;
            }
        }

        public IEnumerable<T> CreateInstances()
        {
            foreach (var ctor in _constructors)
            {
                yield return ctor.Value();
            }
        }

        public T CreateInstance(TKey key)
        {
            if (!_constructors.TryGetValue(key, out var constructorDelegate))
            {
                throw new ArgumentException($"Couldn't find any class that implements type {typeof(T).Name} and has the key {key}.", nameof(key));
            }

            return constructorDelegate();
        }
        
        private static Func<T> CreateConstructorDelegate(Type implementationType)
        {
            var constructorInfo = implementationType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

            return (Func<T>)CreateDelegate<T>(constructorInfo);
        }
    }
}
