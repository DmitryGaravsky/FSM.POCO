namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class TransitionExtension {
        readonly static IDictionary<MethodInfo, Action<object[]>> transitions = new Dictionary<MethodInfo, Action<object[]>>();
        public static Action<object[]> Create(Type type, MethodInfo mInfo) {
            Action<object[]> transition;
            if(!transitions.TryGetValue(mInfo, out transition)) {
                var dynamicMethod = new DynamicMethod(TriggerExtension.GetTriggerName(mInfo),
                    MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                    typeof(void), new Type[] { typeof(object[]) }, type, true);
                Source.BuildTransition(mInfo, dynamicMethod.GetILGenerator(), isStaticCall: true);
                transition = (Action<object[]>)dynamicMethod.CreateDelegate(typeof(Action<object[]>));
                transitions.Add(mInfo, transition);
            }
            return transition;
        }
    }
}