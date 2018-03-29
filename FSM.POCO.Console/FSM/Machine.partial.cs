﻿namespace FSM.POCO.Console {
    using System;
    using System.Collections.Generic;

    // This part demonstrates the code generated by the FSMSource.Create method
    partial class Machine {
        enum Trigger {
            Start,
            Success,
            Failure,
            Retry
        }
        //
        readonly FSM.POCO.Internal.IDispatcher<State> dispatcher;
        public Machine() {
            var idleActions = new Dictionary<Enum, Action<object[]>> {
                    { Trigger.Start, _ => OnStart() },
                };
            var fetchingActions = new Dictionary<Enum, Action<object[]>> {
                    { Trigger.Success, x => OnSuccess((int)x[0]) },
                    { Trigger.Failure, x => OnFailure((Exception)x[0]) },
                };
            var errorActions = new Dictionary<Enum, Action<object[]>> {
                    { Trigger.Retry, _ => OnRetry() },
                };
            var transisions = new Dictionary<State, IDictionary<Enum, Action<object[]>>> { 
                { State.Idle, idleActions },
                { State.Fetching, fetchingActions },
                { State.Error, errorActions },
            };
            var settings = new FSM.POCO.Internal.DispatchersSettings<State, Trigger>();
            dispatcher = new FSM.POCO.Internal.Dispatcher<State>(transisions, settings);
        }
    }
}