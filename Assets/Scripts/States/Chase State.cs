
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ChaseState : State
{
    //TODO: How do you make Update Active only when this script is returned?
    public State IdleState;
    [SerializeField] private Transform playerTransform;
    public Transform enemyTransform;
    [SerializeField] private Rigidbody2D rb;
    private Vector2 lastPosition;
    private Vector2 direction;
    public float speed;
    public float wait;

    #region Player Detection
    [SerializeField] private float playerRadius;
    [SerializeField] private LayerMask playerMask;
    #endregion

    public enum EnemyBehavior
    {
        WeChilling, // will be the default enum so we don't start at any of the other states by default
        SightLost, //if the enemy lost sight of the player
        Charge, //for the touch enemy: Should charge towards the player at full force, giving them damage (knockback maybe?)
        EmitSmell, //for smell enemy starts emitting the fumes and the player starts to love health
        StartSound, //for the sound enemy, will start emitting damanging sound if player is with range.
    }

    public EnemyBehavior attackType;

  >
    public override State RunCurrentState()
    { 
        switch(attackType)
        {
            default:
                Debug.Log("Chase Activated");
                Movement();
                StartCoroutine(FindLastPosition(wait));
                return this;
            case EnemyBehavior.SightLost:
                Debug.Log("Sight Lost");
                return IdleState;
            
        }
        //return this;
        
    }

    public void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        wait = 1f;
        
    }

    public void Movement()
    {
        direction = (lastPosition - (Vector2) rb.position).normalized;
        rb.velocity = (direction * Time.deltaTime * speed);
    }

    /// <summary>
    /// finds the position of the player
    /// </summary>
    /// <param name="duration"> wait time between finding player</param>
    public IEnumerator FindLastPosition(float duration)
    {
        while (true)
        {
            yield return new WaitForSeconds(duration);
            lastPosition = playerTransform.position;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(enemyTransform.transform.position, playerRadius);

    }
    public bool isPlayerNear()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(enemyTransform.transform.position, playerRadius, playerMask);
        if (playerCol != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}