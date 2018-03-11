using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Ckode.ServiceLocator
{
    public abstract class BaseServiceLocator
    {
        protected static ICollection<Type> Types { get; private set; }

        static BaseServiceLocator()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies.SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return new Type[0];
                }
            });

            var classTypes = types.Where(type => type.IsClass && !type.IsAbstract);

            Types = classTypes.ToList().AsReadOnly();
        }

        protected static Delegate CreateDelegate<T>(ConstructorInfo constructor)
        {
            Type delegateType = typeof(Func<T>);
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            // Validate the delegate return type
            MethodInfo delMethod = delegateType.GetMethod("Invoke");
            if (!delMethod.ReturnType.IsAssignableFrom(constructor.DeclaringType))
            {
                throw new InvalidOperationException("The return type of the delegate must be assignable from the constructors declaring type.");
            }
            ParameterInfo[] constructorParam = constructor.GetParameters();

            // Validate the signatures
            ParameterInfo[] delParams = delMethod.GetParameters();
            if (delParams.Length != constructorParam.Length)
            {
                throw new InvalidOperationException("The delegate signature does not match that of the constructor");
            }
            for (int i = 0; i < delParams.Length; i++)
            {
                if (delParams[i].ParameterType != constructorParam[i].ParameterType || delParams[i].IsOut)
                {
                    throw new InvalidOperationException("The delegate signature does not match that of the constructor");
                }
            }
            // Create the dynamic method
            DynamicMethod method =
                new DynamicMethod(
                    string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
                    constructor.DeclaringType,
                    Array.ConvertAll<ParameterInfo, Type>(constructorParam, p => p.ParameterType),
                    true
                );

            // Create the il
            ILGenerator gen = method.GetILGenerator();
            for (int i = 0; i < constructorParam.Length; i++)
            {
                if (i < 4)
                {
                    switch (i)
                    {
                        case 0:
                            gen.Emit(OpCodes.Ldarg_0);
                            break;
                        case 1:
                            gen.Emit(OpCodes.Ldarg_1);
                            break;
                        case 2:
                            gen.Emit(OpCodes.Ldarg_2);
                            break;
                        case 3:
                            gen.Emit(OpCodes.Ldarg_3);
                            break;
                    }
                }
                else
                {
                    gen.Emit(OpCodes.Ldarg_S, i);
                }
            }
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate :)
            return method.CreateDelegate(delegateType);
        }
    }
}
