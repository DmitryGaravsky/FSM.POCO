namespace FSM.POCO {
    using System;
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
            BuildConstructors(baseType, typeBuilder,
                EnsureDispatcherField(typeBuilder, baseType, stateType));
            return typeBuilder.CreateType();
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
        static void BuildConstructors(Type baseType, TypeBuilder typeBuilder, FieldInfo dispatcherField) {
            var cInfos = baseType.GetConstructors(BF.Instance | BF.NonPublic | BF.Public);
            for(int i = 0; i < cInfos.Length; i++)
                CreateConstructor(cInfos[i], typeBuilder, dispatcherField);
        }
        static void CreateConstructor(ConstructorInfo cInfo, TypeBuilder typeBuilder, FieldInfo dispatcherField) {
            Type[] parameterTypes = cInfo.GetParameters()
                .Select(p => p.ParameterType).ToArray();
            var ctorBuilder = typeBuilder.DefineConstructor(MA.Public, CallingConventions.Standard, parameterTypes);
            var ctorGenerator = ctorBuilder.GetILGenerator();
            ctorGenerator.Emit(OpCodes.Ldarg_0);
            EmitLdargs(parameterTypes, ctorGenerator);
            ctorGenerator.Emit(OpCodes.Call, cInfo);
            ctorGenerator.Emit(OpCodes.Ret);
        }
        readonly static OpCode[] args = new OpCode[] { 
            OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 
        };
        static void EmitLdargs(Array parameters, ILGenerator generator) {
            for(int i = 0; i < parameters.Length; i++) {
                if(i < 3)
                    generator.Emit(args[i]);
                else
                    generator.Emit(OpCodes.Ldarg_S, i + 1);
            }
        }
        internal static void BuildTransition(MethodInfo targetMethod, ILGenerator generator, bool isStaticCall = false) {
            var parameters = targetMethod.GetParameters();
            OpCode @this = OpCodes.Ldarg_0;
            OpCode @params = isStaticCall ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
            generator.Emit(@this);
            if(isStaticCall) {
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldelem_Ref); // @this = @params[0]
            }
            int offset = isStaticCall ? 1 : 0;
            generator.DeclareLocal(typeof(int)); // locals: int length
            generator.Emit(@params);
            generator.Emit(OpCodes.Ldlen);
            generator.Emit(OpCodes.Conv_I4);
            generator.Emit(OpCodes.Stloc_0);
            for(int i = 0; i < parameters.Length; i++) {
                ParameterInfo p = parameters[i];
                generator.Emit(OpCodes.Ldloc_0); // @length
                EmitLdIndex(generator, offset + i);
                var labelGetIndexedParam = generator.DefineLabel();
                generator.Emit(OpCodes.Bgt_S, @labelGetIndexedParam);
                // DefaultValue
                EmitLdDefaultValue(generator, p);
                var @labelContinue = generator.DefineLabel();
                generator.Emit(OpCodes.Br_S, @labelContinue);
                // Indexed Parameter
                generator.MarkLabel(@labelGetIndexedParam);
                generator.Emit(@params);
                EmitLdIndex(generator, offset + i);
                generator.Emit(OpCodes.Ldelem_Ref);
                EmitUnboxOrCast(generator, p.ParameterType);
                generator.MarkLabel(@labelContinue);
            }
            generator.Emit(OpCodes.Call, targetMethod);
            generator.Emit(OpCodes.Ret);
        }
        static void EmitUnboxOrCast(ILGenerator generator, Type parameterType) {
            if(parameterType.IsValueType)
                generator.Emit(OpCodes.Unbox, parameterType);
            else {
                if(parameterType != typeof(object))
                    generator.Emit(OpCodes.Castclass, parameterType);
            }
        }
        readonly static OpCode[] indices = new OpCode[] { 
            OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, 
            OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, 
            OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8 
        };
        static void EmitLdIndex(ILGenerator generator, int index) {
            if(index < indices.Length)
                generator.Emit(indices[index]);
            else
                generator.Emit(OpCodes.Ldc_I4_S, index + 1);
        }
        static void EmitLdDefaultValue(ILGenerator generator, ParameterInfo p) {
            if(p.ParameterType.IsClass)
                generator.Emit(OpCodes.Ldnull);
            else
                generator.Emit(OpCodes.Ldc_I4, (int)p.DefaultValue); // TODO
        }
    }
}