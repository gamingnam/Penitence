using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public State chaseState;
    public State poopState;
    public bool canSeePlayer;

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
                //return another state or somthing
                return poopState;
            default:
                return this;

        }
    }

    void Update()
    {
        Debug.Log(baseState);

        if (canSeePlayer || Input.GetKeyDown(KeyCode.G))
        {
            baseState = baseStates.chase;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            baseState = baseStates.wonder;
        }
    }
}