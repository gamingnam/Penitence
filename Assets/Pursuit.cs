using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursuit : State
{
    public override State RunCurrentState()
    {
        return this;
    }
}
