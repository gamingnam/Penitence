
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public State chaseState;
    public State poopState;

    public enum baseStates
    {
        Idle,
        chase,
        wonder,
        stalk
    }

    public baseStates baseState;

    public override State RunCurrentState()
    {
        switch (baseState)
        {
            case baseStates.chase:
                return chaseState;
            case baseStates.wonder:
                return poopState;
            default:
                return this;

        }
       

    }
}