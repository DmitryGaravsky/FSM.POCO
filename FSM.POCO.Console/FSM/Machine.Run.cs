namespace FSM.POCO.Console {
    using FSM.POCO;

    // Public API part
    public partial class Machine {
        public void Run() {
            this.Dispatch(() => OnStart());
        }
    }
}