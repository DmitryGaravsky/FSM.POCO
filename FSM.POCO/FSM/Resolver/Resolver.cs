namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Actions = System.Collections.Generic.IDictionary<System.Enum, System.Action<object[]>>;
    using CreateTransition = System.Func<System.Type, System.Reflection.MethodInfo, System.Action<object[]>>;

    public interface IResolver {
        IDictionary<Enum, string> ResolveTriggers(Type machineType);
    }
    public interface IResolver<TState> : IResolver {
        IDictionary<TState, Actions> ResolveTransitions(Type machineType, CreateTransition createTransition = null);
    }
    //
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class Resolver<TState> : IResolver<TState>, IResolver {
        internal enum Trigger { }
        IDictionary<Enum, string> IResolver.ResolveTriggers(Type machineType) {
            var methods = StateMethods.GetStateMethods(machineType);
            var triggers = new Dictionary<Enum, string>(methods.Length);
            for(int i = 0; i < methods.Length; i++) {
                Trigger key = (Trigger)methods[i].MetadataToken;
                string name;
                if(!triggers.TryGetValue(key, out name))
                    triggers.Add(key, methods[i].@TriggerName());
            }
            return triggers;
        }
        IDictionary<TState, Actions> IResolver<TState>.ResolveTransitions(Type machineType, CreateTransition createTransition) {
            var transitions = new Dictionary<TState, Actions>(7);
            var methods = StateMethods.GetStateMethods(machineType);
            for(int i = 0; i < methods.Length; i++) {
                TState state = methods[i].@GetState<TState>();
                Actions actions;
                if(!transitions.TryGetValue(state, out actions)) {
                    actions = new Dictionary<Enum, Action<object[]>>(3);
                    transitions.Add(state, actions);
                }
                Trigger key = (Trigger)methods[i].MetadataToken;
                if(!actions.ContainsKey(key)) {
                    if(createTransition == null)
                        createTransition = TransitionExtension.Create;
                    var transition = createTransition(machineType, methods[i]);
                    actions.Add(key, transition);
                }
            }
            return transitions;
        }
    }
}