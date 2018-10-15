namespace FSM.POCO {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using FSM.POCO.Internal;
    using BF = System.Reflection.BindingFlags;
    using FA = System.Reflection.FieldAttributes;
    using MA = System.Reflection.MethodAttributes;
    using TA = System.Reflection.TypeAttributes;

    partial class Source {
        readonly static TypesActivator typesActivator = new TypesActivator();
        static Type CreateType(Type baseType, Type stateType) {
            TypeBuilder typeBuilder = GetTypeBuilder(baseType);
            IResolver resolver = CreateResolver(stateType);
            BuildConstructors(typeBuilder, baseType, stateType,
                EnsureDispatcherField(typeBuilder, baseType, stateType),
                EnsureTriggerType(typeBuilder, baseType, resolver));
            var type = typeBuilder.CreateType();
            DynamicTypesHelper.Save();
            return type;
        }
        static IResolver CreateResolver(Type stateType) {
            Type resolverType = typeof(Resolver<>)
                .MakeGenericType(stateType);
            return (IResolver)Activator.CreateInstance(resolverType);
        }
        static TypeBuilder GetTypeBuilder(Type baseType) {
            var moduleBuilder = DynamicTypesHelper.GetModuleBuilder(baseType.Assembly);
            return moduleBuilder.DefineType(DynamicTypesHelper.GetDynamicTypeName(baseType), TA.Public, baseType);
        }
        static FieldInfo EnsureDispatcherField(TypeBuilder typeBuilder, Type baseType, Type stateType) {
            Type dispatcherType = typeof(IDispatcher<>).MakeGenericType(stateType);
            FieldInfo dispatcherField = baseType.GetFields(BF.Public | BF.NonPublic | BF.Instance)
                .Where(f => dispatcherType == f.FieldType).FirstOrDefault();
            return dispatcherField ?? typeBuilder.DefineField("dispatcher", dispatcherType, FA.Private | FA.InitOnly);
        }
        static Type EnsureTriggerType(TypeBuilder typeBuilder, Type baseType, IResolver resolver) {
            Type triggerType = baseType.GetNestedTypes(BF.Public | BF.NonPublic | BF.Instance)
                .Where(t => t.Name == "Trigger").FirstOrDefault();
            return triggerType ?? CreateTriggerType(typeBuilder, baseType, resolver);
        }
        static Type CreateTriggerType(TypeBuilder typeBuilder, Type baseType, IResolver resolver) {
            var triggerTypeBuilder = typeBuilder.DefineNestedType("Trigger", TA.NestedPrivate | TA.Sealed, typeof(Enum), null);
            triggerTypeBuilder.DefineField("value__", typeof(int), FA.Private | FA.SpecialName);
            var triggers = resolver.ResolveTriggers(baseType);
            foreach(KeyValuePair<Enum, string> item in triggers) {
                triggerTypeBuilder.DefineField(item.Value, triggerTypeBuilder, FA.Public | FA.Literal | FA.Static)
                    .SetConstant(Convert.ChangeType(item.Key, typeof(int)));
            }
            return triggerTypeBuilder;
        }
        static void BuildConstructors(TypeBuilder typeBuilder, Type baseType, Type stateType,
            FieldInfo dispatcherField, Type triggerType) {
            TypeBuilder triggerTypeBuilder = triggerType as TypeBuilder;
            if(triggerTypeBuilder != null && !triggerTypeBuilder.IsCreated())
                triggerType = triggerTypeBuilder.CreateType();
            object initialState = MachineExtension.GetInitialState(baseType);
            var transitions = BuildTransitions(typeBuilder, baseType, stateType);
            var cInfos = baseType.GetConstructors(BF.Instance | BF.NonPublic | BF.Public);
            for(int i = 0; i < cInfos.Length; i++) {
                ILGenerator ctorGenerator = EmitBaseCtorCall(typeBuilder, cInfos[i]);
                CreateConstructor(ctorGenerator,
                        stateType, initialState,
                        triggerTypeBuilder, triggerType,
                        dispatcherField, transitions
                    );
            }
        }
        static void CreateConstructor(ILGenerator ctorGenerator,
            Type stateType, object initialState,
            TypeBuilder triggerTypeBuilder, Type triggerType,
            FieldInfo dispatcherField, IDictionary<Enum, HashSet<Transition>> transitions) {
            ctorGenerator.Emit(OpCodes.Ldarg_0);
            // var transisions = new Dictionary<State, IDictionary<Enum, Action<object[]>>>(capacity);
            var transitionsCtor = GetTransitionsCtor(stateType);
            ctorGenerator.Emit(OpCodes.Ldc_I4, triggerType.GetFields().Length - 1);
            ctorGenerator.Emit(OpCodes.Newobj, transitionsCtor);
            ctorGenerator.DeclareLocal(transitionsCtor.DeclaringType);
            ctorGenerator.Emit(OpCodes.Stloc_0);
            // Transitions
            AddActions(ctorGenerator, stateType, triggerTypeBuilder, transitions, transitionsCtor.DeclaringType);
            ctorGenerator.Emit(OpCodes.Ldloc_0);
            // var settings = new Internal.DispatchersSettings<State, Trigger>(initialState);
            var dispatcherSettingCtor = GetDispatcherSettingsCtor(stateType, triggerTypeBuilder, triggerType);
            ctorGenerator.EmitLdEnum(stateType, initialState);
            ctorGenerator.Emit(OpCodes.Newobj, dispatcherSettingCtor);
            // this.dispatcher = new Internal.Dispatcher<State>(transisions, settings);
            var dispatcherCtor = GetDispatcherCtor(stateType);
            ctorGenerator.Emit(OpCodes.Newobj, dispatcherCtor);
            ctorGenerator.Emit(OpCodes.Stfld, dispatcherField);
            ctorGenerator.Emit(OpCodes.Ret);
        }
        static ILGenerator EmitBaseCtorCall(TypeBuilder typeBuilder, ConstructorInfo cInfo) {
            Type[] parameterTypes = cInfo.GetParameters()
                .Select(p => p.ParameterType).ToArray();
            var ctorBuilder = typeBuilder.DefineConstructor(MA.Public, CallingConventions.Standard, parameterTypes);
            var ctorGenerator = ctorBuilder.GetILGenerator();
            ctorGenerator.Emit(OpCodes.Ldarg_0);
            ctorGenerator.EmitLdargs(parameterTypes.Length);
            ctorGenerator.Emit(OpCodes.Call, cInfo);
            return ctorGenerator;
        }
        internal static void BuildTransition(MethodInfo targetMethod, ILGenerator generator, bool isStaticCall) {
            BuildTransition(targetMethod, generator, targetMethod.GetParameters(), isStaticCall);
        }
        static ConstructorInfo GetTransitionsCtor(Type stateType) {
            Type transitionsType = typeof(Dictionary<,>)
                .MakeGenericType(stateType, typeof(IDictionary<Enum, Action<object[]>>));
            return transitionsType.GetConstructor(new Type[] { typeof(int) });
        }
        readonly static Type DispatcherSettingTypeDefinition = typeof(DispatchersSettings<,>);
        readonly static Type IDispatcherSettingTypeDefinition = typeof(IDispatchersSettings<>);
        readonly static Type DispatcherTypeDefinition = typeof(Dispatcher<>);
        static ConstructorInfo GetDispatcherSettingsCtor(Type stateType, TypeBuilder triggerTypeBuilder, Type triggerType) {
            Type dispatcherSettingType = DispatcherSettingTypeDefinition
                .MakeGenericType(stateType, triggerTypeBuilder ?? triggerType);
            if(triggerTypeBuilder == null)
                return dispatcherSettingType.GetConstructor(new Type[] { stateType, triggerType });
            var dispatcherSettingCtor = DispatcherSettingTypeDefinition
                .GetConstructor(new Type[] { DispatcherSettingTypeDefinition.GetGenericArguments()[0] });
            return TypeBuilder.GetConstructor(dispatcherSettingType, dispatcherSettingCtor);
        }
        static ConstructorInfo GetDispatcherCtor(Type stateType) {
            Type dispatcherSettingType = IDispatcherSettingTypeDefinition
                .MakeGenericType(stateType);
            Type transitionsType = typeof(IDictionary<,>)
                .MakeGenericType(stateType, typeof(IDictionary<Enum, Action<object[]>>));
            return DispatcherTypeDefinition.MakeGenericType(stateType)
                .GetConstructor(new Type[] { transitionsType, dispatcherSettingType });
        }
    }
}