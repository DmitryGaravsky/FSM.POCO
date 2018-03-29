namespace FSM.POCO {
    using System;

    partial class Source {
        sealed class SourceException : Exception {
            public SourceException(string message)
                : base(message) { }
        }
    }
}