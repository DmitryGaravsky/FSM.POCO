namespace FSM.POCO.Internal {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    static class StateExtension {
        public static TState GetState<TState>(this Expression<Action<object[]>> actionSelector) {
            return (TState)GetStateCore(actionSelector.@Attribute<StateAttribute>().stateObj, typeof(TState));
        }
        public static TState GetState<TState>(this MethodInfo stateMethod) {
            return (TState)GetStateCore(stateMethod.@Attribute<StateAttribute>().stateObj, typeof(TState));
        }
        public static Type GetStateType(Type machineType) {
            Type[] interfaces = machineType.GetInterfaces();
            for(int i = 0; i < interfaces.Length; i++) {
                Type type = interfaces[i];
                if(!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IPOCOMachine<>))
                    continue;
                return type.GetGenericArguments()[0];
            }
            throw new NotSupportedException("Unable to resolve state from " + machineType.Name + " type.");
        }
        #region internal
        internal static object GetStateCore(MethodInfo stateMethod, Type stateType) {
            return GetStateCore(stateMethod.@Attribute<StateAttribute>().stateObj, stateType);
        }
        internal static object GetStateCore(object stateObj, Type stateType) {
            Type objectType = stateObj.GetType();
            if(stateType.IsAssignableFrom(objectType))
                return stateObj;
            if(stateType.IsEnum) {
                if(stateObj is string)
                    return Enum.Parse(stateType, (string)stateObj, true);
                if(Enum.GetUnderlyingType(stateType) == stateType)
                    return Enum.ToObject(stateType, stateObj);
            }
            if(typeof(IConvertible).IsAssignableFrom(stateType))
                return Convert.ChangeType(stateObj, stateType);
            throw new NotSupportedException("Unable to resolve state from {" + stateObj.ToString() + "} value.");
        }
        #endregion internal
    }
}