using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    /// <summary>
    /// A function used to run whatever behavior the state is executing 
    /// (basically acts as our void update in our individual states)
    /// </summary>
    /// <returns>the state we're currently in or another state</returns>

    public abstract State RunCurrentState();
}