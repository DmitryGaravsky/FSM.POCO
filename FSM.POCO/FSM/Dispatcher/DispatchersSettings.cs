namespace FSM.POCO.Internal {
    using System;
    using System.ComponentModel;

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDispatchersSettings<TState> {
        Type TriggerType { get; }
        TState InitialState { get; }
    }
    //
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DispatchersSettings<TState, TTrigger> : IDispatchersSettings<TState>
        where TTrigger : struct {
        readonly TState initialState;
        public DispatchersSettings(TState initialState = default(TState)) {
            this.initialState = initialState;
        }
        Type IDispatchersSettings<TState>.TriggerType {
            get { return typeof(TTrigger); }
        }
        TState IDispatchersSettings<TState>.InitialState {
            get { return initialState; }
        }
    }
}