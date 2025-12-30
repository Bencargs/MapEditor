using System;
using System.Collections.Generic;
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
            ( State.Idle, State.Moving ) => true,
            ( State.Idle, State.Attacking ) => true,
            ( State.Idle, State.Loading ) => true,
            ( State.Idle, State.Unloading ) => true,

            ( State.Moving, State.Idle ) => true,
            ( State.Moving, State.Stopping ) => true,
            ( State.Moving, State.Loading ) => true,
     
            ( State.Loading, State.Idle ) => true,
            ( State.Unloading, State.Idle ) => true,

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