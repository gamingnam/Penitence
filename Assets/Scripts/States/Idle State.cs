using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State, IEnemy
{
    public State chaseState;
    public State poopstate;
    public bool canSeePlayer;

    enum skibidiEnum
    {
        chase,
        wonder,
        stalk
    }

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

        switch (skibidiEnum)
        {
            case skibidiEnum.chase:
                return chaseState;
            case skibidiEnum.wonder:
                return poopstate;
            break;

        }
    }
}