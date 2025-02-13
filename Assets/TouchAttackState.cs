using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchAttackState : State
{
    #region General
    [Header("General")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool hasTouchAttacked;
    [SerializeField] private float coolDownSeconds;
    #endregion

    #region AStarGrid and Scripts
    [Header("AStarGrid and Scripts")]
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private AIPath aiPath;
    #endregion

    #region States to Transition to
    [Header("States to Transition to")]
    [SerializeField] private State wanderState;
    [SerializeField] private State pursuitState;
    #endregion

    void Start()
    {
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiPath = enemy.GetComponent<AIPath>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override State RunCurrentState()
    {
        if (!hasTouchAttacked)
        {
            StartCoroutine(TouchAttack());
        }
        else 
        {
            StopCoroutine(TouchAttack());
            return pursuitState;
        }

        return this; 
    }


    private IEnumerator TouchAttack() 
    {
        aiDestinationSetter.enabled = false;
        aiPath.enabled = false;
        Lunge();
        rb.AddForce(Vector2.zero, ForceMode2D.Impulse);
        yield return new WaitForSeconds(coolDownSeconds);
        aiDestinationSetter.enabled = true;
        aiPath.enabled = true;
        hasTouchAttacked = true;


    }
    
    private void Lunge() 
    {
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        rb.AddForce(directionToPlayer * 3f, ForceMode2D.Impulse);
    }
}
