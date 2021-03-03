using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Ckode.ServiceLocator
{
    public abstract class BaseServiceLocator
    {
        protected static ICollection<Type> Types { get; }

        /// <summary>
        /// List of any assemblies the locator failed to load types from along with their exception.
        /// Useful for debugging cases where the ServiceLocator cannot find a type you know should exist.
        /// </summary>
        public static IList<string> FailedAssemblies { get; }

        static BaseServiceLocator()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var failed = new List<string>();

            var types = assemblies.SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (Exception ex)
                {
                    failed.Add($"{assembly.FullName}: {ex.ToString()}");
                    return new Type[0];
                }
            });

            FailedAssemblies = failed.ToArray();

            var classTypes = types.Where(type => type.IsClass && !type.IsAbstract);

            Types = classTypes.ToList().AsReadOnly();
        }

        protected static Delegate CreateDelegate(ConstructorInfo constructor, Type returnType)
        {
            var delegateType = typeof(Func<>).MakeGenericType(returnType);
            return CreateDelegateInternal(constructor, delegateType);
        }

        protected static Delegate CreateDelegate<T>(ConstructorInfo constructor)
        {
            var delegateType = typeof(Func<T>);
            return CreateDelegateInternal(constructor, delegateType);
        }

        private static Delegate CreateDelegateInternal(ConstructorInfo constructor, Type delegateType)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            // Validate the delegate return type
            var delMethod = delegateType.GetMethod("Invoke");
            if (!delMethod.ReturnType.IsAssignableFrom(constructor.DeclaringType))
            {
                throw new InvalidOperationException("The return type of the delegate must be assignable from the constructors declaring type.");
            }
            var constructorParam = constructor.GetParameters();

            // Create the dynamic method
            var method =
                new DynamicMethod(
                    string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
                    constructor.DeclaringType,
                    Array.ConvertAll<ParameterInfo, Type>(constructorParam, p => p.ParameterType),
                    true
                );

            // Create the il
            var gen = method.GetILGenerator();
           
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate :)
            return method.CreateDelegate(delegateType);
        }
    }
}
