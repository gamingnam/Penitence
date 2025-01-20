
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public State chaseState;
    public State poopState;

    [SerializeField] private Transform enemyTransform;
    [SerializeField] private float playerRadius;   
    [SerializeField] private LayerMask playerMask;
    public bool showGizmos;

    public override State RunCurrentState()
    {

        Debug.Log(isPlayerNear());
        if (isPlayerNear())
        {
            showGizmos = false;
            return chaseState;
            
        }
        else
        {
            showGizmos = true;
        }
        return this;
    }

    private bool isPlayerNear()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, playerRadius, playerMask);
    }

    private void OnDrawGizmos()
    {   
        if(showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(enemyTransform.position, playerRadius);
        }
    }


}