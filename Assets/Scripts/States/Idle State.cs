
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

        if (isPlayerNear())
        {
            baseState = baseStates.chase;
        }
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

    private bool isPlayerNear()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, playerRadius, playerMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(enemyTransform.position, playerRadius);
    }


}