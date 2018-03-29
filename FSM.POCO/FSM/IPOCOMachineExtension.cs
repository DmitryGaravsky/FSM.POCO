namespace FSM.POCO {
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using Internal;

    public static class IPOCOMachineExtension {
        public static bool Is<TState>(this IPOCOMachine<TState> machine, TState state) {
            return GetDispatcher(machine).@Get(x => 
                object.Equals(x.Current, state));
        }
        public static void SetState<TState>(this IPOCOMachine<TState> machine, TState state) {
            GetDispatcher(machine).@Do(x =>
                x.SetState(state));
        }
        public static void Dispatch<TState>(this IPOCOMachine<TState> machine,
            Expression<Action> selector) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.@GetTrigger(x.Settings.TriggerType)));
        }
        public static void Dispatch<TState, T>(this IPOCOMachine<TState> machine,
            Expression<Action<T>> selector, T parameter) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.@GetTrigger(x.Settings.TriggerType), parameter));
        }
        public static void Dispatch<TState, T1, T2>(this IPOCOMachine<TState> machine,
            Expression<Action<T1, T2>> selector, T1 parameter1, T2 parameter2) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.@GetTrigger(x.Settings.TriggerType), parameter1, parameter2));
        }
        public static void Dispatch<TState, T1, T2, T3>(this IPOCOMachine<TState> machine,
            Expression<Action<T1, T2, T3>> selector, T1 parameter1, T2 parameter2, T3 parameter3) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.@GetTrigger(x.Settings.TriggerType), parameter1, parameter2, parameter3));
        }
        public static void Dispatch<TState>(this IPOCOMachine<TState> machine,
            Expression<Action<object[]>> selector, params object[] parameters) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.@GetTrigger(x.Settings.TriggerType), parameters));
        }
        static Internal.IDispatcher<TState> GetDispatcher<TState>(IPOCOMachine<TState> machine) {
            return DispatcherAccessor<TState>.Get(machine.GetType())(machine);
        }
    }
}