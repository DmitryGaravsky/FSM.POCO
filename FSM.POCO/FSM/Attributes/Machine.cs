namespace FSM.POCO {
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MachineAttribute : Attribute {
        internal readonly object initialStateObj;
        public MachineAttribute(object initialState = null) {
            if(object.ReferenceEquals(initialState, string.Empty))
                initialState = null;
            if(initialState is string && string.IsNullOrWhiteSpace((string)initialState))
                initialState = null;
            this.initialStateObj = initialState;
        }
    }
}