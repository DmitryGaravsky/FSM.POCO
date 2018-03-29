namespace FSM.POCO.Internal {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    static class StateExtension {
        public static TState GetState<TState>(this Expression<Action<object[]>> actionSelector) {
            return GetStateCore<TState>(actionSelector.@Attribute<StateAttribute>().stateObj);
        }
        public static TState GetState<TState>(this MethodInfo stateMethod) {
            return GetStateCore<TState>(stateMethod.@Attribute<StateAttribute>().stateObj);
        }
        static TState GetStateCore<TState>(object stateObj) {
            Type stateType = typeof(TState), objectType = stateObj.GetType();
            if(stateType.IsAssignableFrom(objectType))
                return (TState)stateObj;
            if(stateType.IsEnum) {
                if(stateObj is string)
                    return (TState)Enum.Parse(stateType, (string)stateObj, true);
                if(Enum.GetUnderlyingType(stateType) == stateType)
                    return (TState)Enum.ToObject(stateType, stateObj);
            }
            if(typeof(IConvertible).IsAssignableFrom(stateType))
                return (TState)Convert.ChangeType(stateObj, stateType);
            throw new NotSupportedException("Unable to resolve state from {" + stateObj.ToString() + "} value.");
        }
        public static Type GetStateType(Type machineType) {
            Type[] interfaces = machineType.GetInterfaces();
            for(int i = 0; i < interfaces.Length; i++) {
                Type type = interfaces[i];
                if(!type.IsGenericType || typeof(IPOCOMachine<>) != type.GetGenericTypeDefinition())
                    continue;
                return type.GetGenericArguments()[0];
            }
            throw new NotSupportedException("Unable to resolve state from " + machineType.Name + " type.");
        }
    }
}