namespace FSM.POCO {
    using System;
    using System.Linq.Expressions;
    using Internal;

    public static class IPOCOMachineExtension {
        public static void SetState<TState>(this IPOCOMachine<TState> machine, TState state) {
            GetDispatcher(machine).@Do(x =>
                x.SetState(state));
        }
        public static void Dispatch<TState>(this IPOCOMachine<TState> machine,
            Expression<Action<object[]>> selector, params object[] parameters) {
            GetDispatcher(machine).@Do(x =>
                x.Dispatch(selector.GetTrigger(x.Settings.TriggerType), parameters));
        }
        static Internal.IDispatcher<TState> GetDispatcher<TState>(IPOCOMachine<TState> machine) {
            return DispatcherAccessor<TState>.Get(machine.GetType())(machine);
        }
    }
}