namespace FSM.POCO {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using FSM.POCO.Internal;
    using MA = System.Reflection.MethodAttributes;

    partial class Source {
        sealed class Transition : IEquatable<Transition> {
            public readonly int Trigger;
            public Transition(int trigger) {
                this.Trigger = trigger;
            }
            public MethodInfo Method;
            public bool Equals(Transition info) {
                if(info == null)
                    return false;
                return Trigger.Equals(info.Trigger) && (Method == info.Method);
            }
            public sealed override bool Equals(object obj) {
                return Equals((Transition)obj);
            }
            public sealed override int GetHashCode() {
                return Trigger;
            }
        }
        //
        static IDictionary<Enum, HashSet<Transition>> BuildTransitions(TypeBuilder typeBuilder, Type machineType, Type stateType) {
            var methods = StateMethods.GetStateMethods(machineType);
            var transitions = new Dictionary<Enum, HashSet<Transition>>(7);
            for(int i = 0; i < methods.Length; i++) {
                Enum state = (Enum)StateExtension.GetStateCore(methods[i], stateType);
                HashSet<Transition> actions;
                if(!transitions.TryGetValue(state, out actions)) {
                    actions = new HashSet<Transition>();
                    transitions.Add(state, actions);
                }
                var info = new Transition(methods[i].MetadataToken);
                if(!actions.Contains(info)) {
                    info.Method = CreateTransition(typeBuilder, methods[i]);
                    actions.Add(info);
                }
            }
            return transitions;
        }
        static MethodInfo CreateTransition(TypeBuilder typeBuilder, MethodInfo targetMethod) {
            var transition = typeBuilder.DefineMethod("<.ctor>b__" + targetMethod.Name,
                MA.Private | MA.HideBySig, CallingConventions.Standard, typeof(void), new Type[] { typeof(object[]) });
            var generator = transition.GetILGenerator();
            var parameters = targetMethod.GetParameters();
            if(parameters.Length == 0) {
                BuildTransition(targetMethod, generator);
                return transition;
            }
            if(parameters.Length == 1) {
                BuildTransition(targetMethod, generator, parameters[0].ParameterType);
                return transition;
            }
            BuildTransition(targetMethod, generator, parameters, false);
            return transition;
        }
        static void BuildTransition(MethodInfo targetMethod, ILGenerator generator) {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, targetMethod);
            generator.Emit(OpCodes.Ret);
        }
        static void BuildTransition(MethodInfo targetMethod, ILGenerator generator, Type parameterType) {
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldlen);
            var labelGetIndexedParam = generator.DefineLabel();
            generator.Emit(OpCodes.Brtrue_S, @labelGetIndexedParam);
            // Default
            generator.EmitLdDefaultValue(parameterType);
            var @labelContinue = generator.DefineLabel();
            generator.Emit(OpCodes.Br_S, @labelContinue);
            // Unbox Or Cast from Array
            generator.MarkLabel(@labelGetIndexedParam);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Ldelem_Ref);
            generator.EmitUnboxOrCast(parameterType);
            //
            generator.MarkLabel(@labelContinue);
            generator.Emit(OpCodes.Callvirt, targetMethod);
            generator.Emit(OpCodes.Ret);
        }
        static void BuildTransition(MethodInfo targetMethod, ILGenerator generator, ParameterInfo[] parameters, bool isStaticCall) {
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
                generator.EmitLdIndex(offset + i);
                var labelGetIndexedParam = generator.DefineLabel();
                generator.Emit(OpCodes.Bgt_S, @labelGetIndexedParam);
                // DefaultValue
                generator.EmitLdDefaultValue(p.ParameterType);
                var @labelContinue = generator.DefineLabel();
                generator.Emit(OpCodes.Br_S, @labelContinue);
                // Indexed Parameter
                generator.MarkLabel(@labelGetIndexedParam);
                generator.Emit(@params);
                generator.EmitLdIndex(offset + i);
                generator.Emit(OpCodes.Ldelem_Ref);
                generator.EmitUnboxOrCast(p.ParameterType);
                generator.MarkLabel(@labelContinue);
            }
            generator.Emit(OpCodes.Callvirt, targetMethod);
            generator.Emit(OpCodes.Ret);
        }
    }
}