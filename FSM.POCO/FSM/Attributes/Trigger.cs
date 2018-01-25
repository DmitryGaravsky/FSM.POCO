namespace FSM.POCO {
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TriggerAttribute : Attribute {
        internal readonly string name;
        internal readonly bool isTrigger = true;
        public TriggerAttribute(bool isTrigger) {
            this.isTrigger = isTrigger;
        }
        public TriggerAttribute(string name) {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentException("The name parameter cannot be empty");
            this.name = name;
        }
    }
}