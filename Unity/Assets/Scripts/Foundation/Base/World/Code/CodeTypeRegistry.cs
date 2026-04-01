using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;

namespace Chaos
{
    public sealed class CodeTypeRegistry : Singleton<CodeTypeRegistry>, ISingletonAwake<Assembly[]>
    {
        private readonly Dictionary<string, Type> allTypes = new();
        private readonly HashSetDictionary<Type, Type> types = new();

        public void Awake(Assembly[] assemblies)
        {
            foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes()))
            {
                allTypes[type.FullName!] = type;

                if (type.IsAbstract)
                {
                    continue;
                }

                foreach (var attribute in type.GetCustomAttributes<BaseAttribute>(true))
                {
                    types.Add(attribute.GetType(), type);
                }
            }
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            return !types.ContainsKey(systemAttributeType) ? new HashSet<Type>() : types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes() => allTypes;

        public Type GetType(string typeName)
        {
            allTypes.TryGetValue(typeName, out var type);
            return type;
        }

        public void Execute()
        {
            foreach (var type in GetTypes(typeof(CodeProcessAttribute)))
            {
                var instance = Activator.CreateInstance(type);
                ((ISingletonAwake)instance).Awake();
                World.Default.AddSingleton((Singleton)instance);
            }
        }
    }
}