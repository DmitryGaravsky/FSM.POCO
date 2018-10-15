namespace FSM.POCO {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    partial class Source {
        readonly static Type actionsType = typeof(Dictionary<Enum, Action<object[]>>);
        readonly static ConstructorInfo actionsCtor = actionsType
            .GetConstructor(new Type[] { typeof(int) });
        readonly static ConstructorInfo actionCtor = typeof(Action<object[]>)
            .GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
        readonly static MethodInfo addAction = actionsType
            .GetMethod("Add", new Type[] { typeof(Enum), typeof(Action<object[]>) });
        //
        static void AddActions(ILGenerator ctorGenerator, Type stateType, Type triggerType,
            IDictionary<Enum, HashSet<Transition>> transitions, Type transitionsType) {
            var addTransition = transitionsType
                .GetMethod("Add", new Type[] { stateType, typeof(IDictionary<Enum, Action<object[]>>) });
            var actions = ctorGenerator.DeclareLocal(actionsType);
            foreach(var entry in transitions) {
                // var actions = new Dictionary<Enum, Action<object[]>>(count);
                ctorGenerator.Emit(OpCodes.Ldc_I4, entry.Value.Count);
                ctorGenerator.Emit(OpCodes.Newobj, actionsCtor);
                ctorGenerator.Emit(OpCodes.Stloc_1);
                foreach(var transition in entry.Value) {
                    ctorGenerator.Emit(OpCodes.Ldloc_1);
                    ctorGenerator.Emit(OpCodes.Ldc_I4, transition.Trigger);
                    ctorGenerator.Emit(OpCodes.Box, triggerType);
                    // new Action<object[]>(this, transition.Method)
                    ctorGenerator.Emit(OpCodes.Ldarg_0);
                    ctorGenerator.Emit(OpCodes.Ldftn, transition.Method);
                    ctorGenerator.Emit(OpCodes.Newobj, actionCtor);
                    // actions.Add(trigger, action)
                    ctorGenerator.Emit(OpCodes.Callvirt, addAction);
                }
                // transitions.Add(state, actions)
                ctorGenerator.Emit(OpCodes.Ldloc_0);
                ctorGenerator.EmitLdEnum(stateType, entry.Key);
                ctorGenerator.Emit(OpCodes.Ldloc_1);
                ctorGenerator.Emit(OpCodes.Callvirt, addTransition);
            }
        }
    }
}