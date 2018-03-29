namespace FSM.POCO {
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TriggerAttribute : Attribute {
        internal readonly string name;
        public TriggerAttribute(string name) {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentException("The name parameter cannot be empty");
            this.name = name;
        }
    }
}