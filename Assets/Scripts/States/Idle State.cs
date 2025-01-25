
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{

    #region General
    [Header("General")]
    public bool showGizmos;
    #endregion

    #region States to transition to
    [Header("States to transition to")]
    public State chaseState;
    public State attackState;
    #endregion

    #region Tracking the Player
    [Header("Tracking the Player")]
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private float playerRadius;   
    [SerializeField] private LayerMask playerMask;
    #endregion
    

    public override State RunCurrentState()
    {
        if (isPlayerNear())
        {
            showGizmos = false;
            if(enemy.gameObject.tag == "StaticEnemy")
            {
                return attackState;
            }
            else
            {
                return chaseState; 
            }
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