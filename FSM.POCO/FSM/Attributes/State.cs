namespace FSM.POCO {
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StateAttribute : Attribute {
        internal readonly object stateObj;
        public StateAttribute(object state) {
            if(object.ReferenceEquals(state, null))
                throw new ArgumentNullException("state");
            if(object.ReferenceEquals(state, string.Empty))
                throw new ArgumentException("The state parameter cannot be empty");
            this.stateObj = state;
        }
    }
}