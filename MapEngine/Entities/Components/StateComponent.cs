using Common;
using Common.Entities;

namespace MapEngine.Entities.Components;

public class StateComponent : IComponent
{
    public ComponentType Type => ComponentType.State;
    public State CurrentState { get; set; }

    private static bool StateTransitions(State from, State to)
    {
        return (from, to) switch
        {
            ( State.Standby, State.Moving ) => true,
            ( State.Standby, State.Attacking ) => true,
            ( State.Standby, State.Loading ) => true,
            ( State.Standby, State.Unloading ) => true,

            ( State.Moving, State.Moving ) => true, // Eg. different move location
            ( State.Moving, State.Standby ) => true,
            ( State.Moving, State.Stopping ) => true,
            ( State.Moving, State.Loading ) => true,
     
            ( State.Loading, State.Standby ) => true,
            ( State.Unloading, State.Standby ) => true,
            
            ( State.Loaded, State.Unloading ) => true,

            _ => false
        };
    }

    public bool CanTransition(State state)
    {
        var isValid = StateTransitions(CurrentState, state);
        return isValid;
    }

    public void ChangeState(State state)
    {
        if (CanTransition(state))
        {
            CurrentState = state;
        }
    }
    
    public IComponent Clone()
    {
        return new StateComponent
        {
            CurrentState = CurrentState
        };
    }
}