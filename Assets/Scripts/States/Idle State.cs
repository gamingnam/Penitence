using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public State chaseState;
    public State poopstate;
    public bool canSeePlayer;

    public enum baseStates
    {
        chase,
        wonder,
        stalk
    }

    public baseStates baseState;

    public override State RunCurrentState()
    {
        if (canSeePlayer)
        {
            return chaseState;
        }
        else
        {
            return this;
        }

        switch (baseState)
        {
            case baseStates.chase:
                return chaseState;
            case baseStates.wonder:
                //return another state or somthing
            default:
                return this;

        }
    }
}