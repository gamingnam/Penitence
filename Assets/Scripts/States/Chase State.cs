
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    //TODO: How do you make Update Active only when this script is returned?
    public State IdleState;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    private Vector2 lastPosition;
    private Vector2 direction;
    public float speed;
    public float wait;

    public enum EnemyBehavior
    {
        WeChilling, // will be the default enum so we don't start at any of the other states by default
        SightLost, //if the enemy lost sight of the player
        Charge, //for the touch enemy: Should charge towards the player at full force, giving them damage (knockback maybe?)
        EmitSmell, //for smell enemy starts emitting the fumes and the player starts to love health
        StartSound, //for the sound enemy, will start emitting damanging sound if player is with range.
    }

    public EnemyBehavior attackType;

    public override State RunCurrentState()
    {
     
        Debug.Log("Chase Activated");
        Movement();
        StartCoroutine(FindLastPosition(wait));

        switch(attackType)
        {
            default:
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
        Gizmos.DrawWireSphere(lastPosition, 0.2f);
       
    }
}