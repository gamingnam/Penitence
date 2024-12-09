using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    public State next;

    public bool nextActive;

    public override State RunCurrentState()
    {
        if(nextActive)
        {
            return next;
        }
        else
        {
            return this;
        }
    }
}