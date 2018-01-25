namespace FSM.POCO.Internal {
    using System;
    using System.Linq.Expressions;

    static class StateExtension {
        public static TState GetState<TState>(this Expression<Action<object[]>> actionSelector) {
            object stateObj = actionSelector.Attribute<StateAttribute>().stateObj;
            if(typeof(TState).IsEnum)
                return (TState)Enum.ToObject(typeof(TState), stateObj);
            if(typeof(IConvertible).IsAssignableFrom(typeof(TState)))
                return (TState)Convert.ChangeType(stateObj, typeof(TState));
            throw new NotSupportedException(stateObj.ToString()); // TODO
        }
    }
}