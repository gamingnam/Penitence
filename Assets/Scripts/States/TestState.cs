using UnityEngine;

public class TestState : State, IEnemy
{

    public override State RunCurrentState()
    {
        Debug.Log("Hello");
        return this;
        
    }
}
