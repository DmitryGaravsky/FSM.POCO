namespace FSM.POCO.Console {
    using System;
    using FSM.POCO;

    // FSM POCO part
    partial class Machine : IPOCOMachine<Machine.State> {
        /*

        {Idle,initial} -onStart-> {Fetching} -onFailure-> {Error} -onRetry-> {Idle}
                                             -onSuccess-> {Idle,exit}
        */
        protected enum State {
            Idle,
            Fetching,
            Error
        }
        [State(State.Idle)]
        protected void OnStart() {
            this.SetState(State.Fetching);
            Console.Clear();
            Console.Write("Please, enter the integer value: ");
            try {
                int data = int.Parse(Console.ReadLine());
                this.Dispatch(x => OnSuccess(x), data);
            }
            catch(Exception e) {
                this.Dispatch(x => OnFailure(x), e);
            }
        }
        [State(State.Fetching)]
        protected void OnSuccess(int data) {
            this.SetState(State.Idle);
            Console.WriteLine("Thanks, your integer is: " + data.ToString());
        }
        [State(State.Fetching)]
        protected void OnFailure(Exception e) {
            this.SetState(State.Error);
            Console.WriteLine("Your input incorrect:" + Environment.NewLine + e.Message);
            this.Dispatch(() => OnRetry());
        }
        [State(State.Error)]
        protected void OnRetry() {
            this.SetState(State.Idle);
            Console.Write("Wanna retry?");
            var keyInfo = Console.ReadKey();
            if(keyInfo.Key == ConsoleKey.Y)
                this.Dispatch(() => OnStart());
        }
    }
}