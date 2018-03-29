namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    static class DynamicTypesHelper {
        static readonly string dynamicSuffix = "Dynamic_" + Guid.NewGuid().ToString();
#if DEBUG
        static IDictionary<string, AssemblyBuilder> aCache = new Dictionary<string, AssemblyBuilder>(StringComparer.Ordinal);
#endif
        static IDictionary<string, ModuleBuilder> mCache = new Dictionary<string, ModuleBuilder>(StringComparer.Ordinal);
        public static TypeBuilder GetTypeBuilder(Type serviceType, Type sourceType) {
            var moduleBuilder = GetModuleBuilder(serviceType.Assembly);
            return moduleBuilder.DefineType(GetDynamicTypeName(sourceType), TypeAttributes.NotPublic, sourceType);
        }
        public static ModuleBuilder GetModuleBuilder(Assembly assembly) {
            AssemblyName assemblyName = assembly.GetName();
            string strAssemblyName = assemblyName.Name;
            ModuleBuilder moduleBuilder;
            if(!mCache.TryGetValue(strAssemblyName, out moduleBuilder)) {
                if(assembly.IsDynamic) {
                    moduleBuilder = mCache.Values
                        .Where(m => AssemblyName.ReferenceMatchesDefinition(m.Assembly.GetName(), assemblyName))
                        .FirstOrDefault();
                }
                if(moduleBuilder == null) {
                    assemblyName = CreateDynamicAssemblyName(strAssemblyName);
#if DEBUG
                    var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                    aCache.Add(strAssemblyName, assemblyBuilder);
#else
                    var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
                    moduleBuilder = assemblyBuilder.DefineDynamicModule(GetDynamicAssemblyName(strAssemblyName));
                }
                mCache.Add(strAssemblyName, moduleBuilder);
            }
            return moduleBuilder;
        }
        static string MakeDynamicAssemblyName(string assemblyName) {
            return assemblyName + "." + dynamicSuffix;
        }
        static AssemblyName CreateDynamicAssemblyName(string assemblyName) {
            return new AssemblyName(MakeDynamicAssemblyName(assemblyName));
        }
        public static string GetDynamicAssemblyName(string assemblyName) {
            return MakeDynamicAssemblyName(assemblyName) + ".dll";
        }
        public static string GetDynamicTypeName(Type type) {
            return GetDynamicTypeName(type, null);
        }
        public static string GetDynamicTypeName(Type type, string typeNameModifier) {
            if(string.IsNullOrEmpty(type.Namespace))
                return GetDynamicTypeName(GetTypeName(type), typeNameModifier);
            return GetDynamicTypeName(type.Namespace + "." + GetTypeName(type), typeNameModifier);
        }
        public static string GetDynamicTypeName(string typeName, string typeNameModifier) {
            if(string.IsNullOrEmpty(typeNameModifier))
                return typeName + "_" + dynamicSuffix;
            return typeName + "_" + typeNameModifier.Replace('.', '_') + "_" + dynamicSuffix;
        }
        public static string GetTypeName(Type type) {
            if(!type.IsGenericType)
                return type.Name;
            var sb = new System.Text.StringBuilder(type.Name);
            int argumentsPos = type.Name.IndexOf('`');
            sb.Remove(argumentsPos, type.Name.Length - argumentsPos);
            sb.Append("<");
            var genericArgs = type.GetGenericArguments();
            for(int i = 0; i < genericArgs.Length; i++) {
                sb.Append(GetTypeName(genericArgs[i]));
                if(i > 0) 
                    sb.Append(",");
            }
            sb.Append(">");
            return sb.ToString();
        }
#if DEBUG
        public static void Save() {
            foreach(var item in aCache) {
                item.Value.Save(GetDynamicAssemblyName(item.Key));
            }
        }
#endif
    }
}