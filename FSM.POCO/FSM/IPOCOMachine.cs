namespace FSM.POCO {
    namespace Internal {
        using System.ComponentModel;
        // All the machines should be markered with this interface
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public interface IPOCOMachine { 
            /* Marker Interface */ 
        }
    }
    // All the machines should be markered with this interface
    public interface IPOCOMachine<out TState> : 
        Internal.IPOCOMachine { 
        /* Marker Interface */ 
    }
}