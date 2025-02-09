using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pursuit : State
{
    #region General
    [Header("General")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform playerTransform;
    #endregion

    #region AStarGrid and Scripts
    [Header("AStarGrid and Scripts")]
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    #endregion

    #region States to Transition to
    [Header("States to Transition to")]
    [SerializeField] private State wanderState;
    public FOV fov;
    #endregion

    #region Ending Pursuit Values
    [Header("Ending Pursuit Values")]
    [SerializeField] private float endPursitTimer;
    [SerializeField] private float endPursitTimerThreshold;
    [SerializeField] private int pursitDistance;
    #endregion

    private void Start()
    {
        fov = enemy.GetComponent<FOV>();
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        enemyTransform = enemy.transform;
    }
    public override State RunCurrentState()
    {
        aiDestinationSetter.target = playerTransform;

        Debug.Log(Vector2.Distance(enemyTransform.position, playerTransform.position));
        /* if (Vector2.Distance(enemyTransform.position, playerTransform.position) >= fov.distance)
        {
            fov.canSeePlayer = false;
        } */

        if (!fov.canSeePlayer)
        {
            aiDestinationSetter.target = null;
            return wanderState;
        }

        return this;
    }

    private void OnDrawGizmos()
    {
        if (showGizmos) 
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(enemyTransform.position, playerTransform.position);
        }
       
    }
}
