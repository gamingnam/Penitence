using UnityEngine;

public class TestState : State
{

    public override State RunCurrentState()
    {
        Debug.Log("Hello");
        return this;
        
    }
}
