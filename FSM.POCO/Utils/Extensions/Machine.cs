namespace FSM.POCO.Internal {
    using System;

    static class MachineExtension {
        public static object GetInitialState(Type machineType) {
            Type stateType = StateExtension.GetStateType(machineType);
            var attribute = machineType.@Attribute<MachineAttribute>(false);
            if(attribute != null && attribute.initialStateObj != null)
                return StateExtension.GetStateCore(attribute.initialStateObj, stateType);
            return Activator.CreateInstance(stateType);
        }
    }
}