# Finite State Machine: POCO extensions


## POCO-Approach

The POCO-approach allows you to avoid writing boilerplate infrastructural code
and focus on writing the business logic.

For example:  
```
{Idle} -onStart-> {Fetching}  -onSuccess-> {Idle}  
                              -onFailure-> {Error} -onRetry-> {Fetching}  
```

Here is a code:
```cs
using FSM.POCO;

public class Machine : IPOCOMachine<Machine.State> {
    protected enum State {
        Idle,
        Fetching,
        Error
    }
    [State(State.Idle)]
    protected void OnStart(params object[] parameters) { 
        /* implementation */  
    }
    [State(State.Fetching)]
    protected void OnSuccess(params object[] parameters) { 
        /* implementation */  
    }
    [State(State.Fetching)]
    protected void OnFailure(params object[] parameters) { 
        /* implementation */  
    }
    [State(State.Error)]
    protected void OnRetry(params object[] parameters) { 
        /* implementation */  
    }
}
```

## FSM-Extensions

The `SetState` and `Dispatch` extension methods in action:

```cs
[State(State.Idle)]
protected void OnStart(params object[] parameters) {
    this.SetState(State.Fetching);
    try {
        int data = /* cool code for fetching some data */42;
        System.Console.WriteLine("Fetched: " + data.ToString());
        this.Dispatch(x => OnSuccess(x), data);
    }
    catch(System.Exception e) {
        this.Dispatch(x => OnFailure(x), e);
    }
}
```
