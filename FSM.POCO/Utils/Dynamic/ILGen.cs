namespace FSM.POCO {
    using System;
    using System.Reflection.Emit;

    static class ILGenExtension {
        readonly static OpCode[] args = new OpCode[] {
            OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3
        };
        public static void EmitLdargs(this ILGenerator generator, int length) {
            for(int i = 0; i < length; i++) {
                if(i < 3)
                    generator.Emit(args[i]);
                else
                    generator.Emit(OpCodes.Ldarg_S, i + 1);
            }
        }
        public static void EmitUnboxOrCast(this ILGenerator generator, Type parameterType) {
            if(parameterType.IsValueType)
                generator.Emit(OpCodes.Unbox_Any, parameterType);
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
        public static void EmitLdIndex(this ILGenerator generator, int index) {
            if(index < indices.Length)
                generator.Emit(indices[index]);
            else
                generator.Emit(OpCodes.Ldc_I4_S, index + 1);
        }
        public static void EmitLdDefaultValue(this ILGenerator generator, Type valueType) {
            if(valueType.IsClass)
                generator.Emit(OpCodes.Ldnull);
            else {
                if(valueType == typeof(int) || valueType == typeof(short) || valueType == typeof(byte)) {
                    generator.Emit(OpCodes.Ldc_I4_0);
                    return;
                }
                if(valueType == typeof(long)) {
                    generator.Emit(OpCodes.Ldc_I4_0);
                    generator.Emit(OpCodes.Conv_I8);
                    return;
                }
                var local = generator.DeclareLocal(valueType);
                generator.Emit(OpCodes.Ldloca, local);
                generator.Emit(OpCodes.Initobj, valueType);
                generator.Emit(OpCodes.Ldloc, local);
            }
        }
        public static void EmitLdEnum(this ILGenerator generator, Type stateType, object value) {
            Type valueType = Enum.GetUnderlyingType(stateType);
            object underlyingValue = Convert.ChangeType(value, valueType);
            if(valueType == typeof(int))
                generator.Emit(OpCodes.Ldc_I4, (int)underlyingValue);
            if(valueType == typeof(long))
                generator.Emit(OpCodes.Ldc_I8, (long)underlyingValue);
            if(valueType == typeof(byte))
                generator.Emit(OpCodes.Ldc_I4_S, (byte)underlyingValue);
            if(valueType == typeof(short))
                generator.Emit(OpCodes.Ldc_I4, (int)(short)underlyingValue);
        }
    }
}