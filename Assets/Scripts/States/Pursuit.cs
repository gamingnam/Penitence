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
    [SerializeField] private FOV fov;
    [SerializeField] private AILerp aiLerp;
    [SerializeField] private float pursuitSpeed;
    #endregion

    #region AStarGrid and Scripts
    [Header("AStarGrid and Scripts")]
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    #endregion

    #region States to Transition to
    [Header("States to Transition to")]
    [SerializeField] private State wanderState;
    [SerializeField] private State attackState;
    [SerializeField] public float attackRange;
    #endregion

    private void Start()
    {
        //aiLerp = enemy.GetComponent<AILerp>();
        fov = enemy.GetComponent<FOV>();
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiLerp = enemy.GetComponent<AILerp>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        enemyTransform = enemy.GetComponent<Transform>();
    }


    public override State RunCurrentState()
    {
        aiLerp.speed = pursuitSpeed;
        aiDestinationSetter.target = playerTransform;
        if (!fov.canSeePlayer)
        {
            aiDestinationSetter.target = null;
            return wanderState;
        }
        else if(fov.canSeePlayer && Vector2.Distance(enemyTransform.position,playerTransform.position) >= fov.distance - attackRange) 
        {
            return attackState;
        }
        return this;
    }
}
