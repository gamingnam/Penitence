
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    //TODO: How do you make Update Active only when this script is returned?
    public State next;
    public bool nextActive;
    [SerializeField] private Transform playerTransform;
    private Vector2 lastPosition;
    [SerializeField] private Rigidbody2D rb;
    private Vector2 direction;
    public float speed;
    public float wait;



    public override State RunCurrentState()
    {
     
        Debug.Log("Chase Activated");
        Movement();
        StartCoroutine(FindLastPosition(wait));
        return this;
        
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