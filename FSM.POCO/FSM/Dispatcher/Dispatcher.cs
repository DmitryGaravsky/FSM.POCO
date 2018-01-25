namespace FSM.POCO.Internal {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDispatcher<TState> {
        IDispatchersSettings<TState> Settings { get; }
        TState Current { get; }
        void SetState(TState state);
        void Dispatch(Enum trigger, params object[] parameters);
    }
    //
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Dispatcher<TState> : IDispatcher<TState> {
        readonly IDispatchersSettings<TState> settings;
        readonly IDictionary<TState, IDictionary<Enum, Action<object[]>>> transitions;
        public Dispatcher(IDictionary<TState, IDictionary<Enum, Action<object[]>>> transisions, IDispatchersSettings<TState> settings) {
            if(settings == null)
                throw new ArgumentNullException("settings");
            if(settings.TriggerType == null)
                throw new ArgumentNullException("settings.TriggerType");
            if(!settings.TriggerType.IsEnum)
                throw new ArgumentException("The settings.TriggerType should be Enum.");
            this.settings = settings;
            this.transitions = transisions ?? new Dictionary<TState, IDictionary<Enum, Action<object[]>>>();
            this.current = settings.InitialState;
        }
        IDispatchersSettings<TState> IDispatcher<TState>.Settings {
            get { return settings; }
        }
        TState current;
        TState IDispatcher<TState>.Current {
            get { return current; }
        }
        void IDispatcher<TState>.SetState(TState state) {
            this.current = state;
        }
        void IDispatcher<TState>.Dispatch(Enum trigger, params object[] parameters) {
            if(settings.TriggerType != trigger.GetType())
                throw new ArgumentException("trigger");
            IDictionary<Enum, Action<object[]>> actions;
            if(transitions.TryGetValue(current, out actions)) {
                Action<object[]> apply;
                if(actions.TryGetValue(trigger, out apply))
                    apply(parameters);
            }
        }
    }
}