using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    /*TODO:
     FIX THE DROPLET SYSTEM WITH THIS: 
     * When the player exists the enemy's radius the player drop an invisble gameobject by the name of droplet. 
     * Afterwards, the enemy will move towards the droplet and see if the player exists at it. 
     * After doing this 2-3 times (AKA spawning 2-3 droplets) the enemy will give up and enter the wander state. 
     */

    #region General
    [Header("General")]
    [SerializeField] private bool showGizmos = true; 
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Rigidbody2D rb;
    #endregion

    #region Steering Weights
    [Header("Steering Weights")]
    [SerializeField] private float seekWeight = 1.5f;
    [SerializeField] private float avoidWeight = 1.0f;
    #endregion

    #region Context
    [Header("Context")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float obstacleDetectionRadius = 3f;
    [SerializeField] private float playerRadius = 5f;
    [SerializeField] private float distanceClamp;
    [SerializeField] private float duration;
    public float speed = 5f;
    #endregion

    #region States to transition to
    [Header("States to transition to")]
    [SerializeField] private State idleState;
    [SerializeField] private State attackState;
    #endregion


    #region Tracking the Player
    [Header("Tracking the Player")]
    [SerializeField] private Vector2 lastKnownPosition;
    [SerializeField] private int dropletCounter;
    [SerializeField] private int maxDroplets;
    [SerializeField] private bool isFollowingDroplets; 
    private Queue<Vector2> droplets = new Queue<Vector2>();
    #endregion

    public void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    public override State RunCurrentState()
    {
        Debug.Log("Chase state is activated");
        if (playerTransform == null)
        {
            Debug.LogWarning("Player not assigned, transitioning to IdleState.");
            return idleState;
        }

        if (isPlayerNear())
        {
            lastKnownPosition = playerTransform.position;

        }
        else if(!isFollowingDroplets && !isPlayerNear())
        {
            StartCoroutine(FollowDroplet(lastKnownPosition,duration));
            isFollowingDroplets = true;
        }

        if(dropletCounter > maxDroplets)
        {
            return idleState;
        }

        // Dynamically adjust weights based on proximity to the last known position
        AdjustWeightsDynamically(Vector2.Distance(rb.position, lastKnownPosition));

        // Perform context-oriented steering to chase the player or move to the last known position
        Vector2 targetPosition = isFollowingDroplets ? lastKnownPosition : playerTransform.position;
        Vector2 steeringForce = CalculateSteeringForce(targetPosition);
        ApplySteering(steeringForce);

        Debug.Log("Chasing player or moving to last known position.");

        foreach (var droplet in droplets)
        {
            Debug.Log(droplet);
        }
        
        return this; // Remain in ChaseState
    }

    public IEnumerator FollowDroplet(Vector2 position, float duration)
    {
        droplets.Enqueue(position);
        dropletCounter++;

        while (true)
        {

            yield return new WaitForSeconds(duration);
            if(droplets.Count >= 0)
            {
                lastKnownPosition = droplets.Peek();
                if (Vector2.Distance(rb.position, lastKnownPosition) < 0.5f)
                {
                    droplets.Dequeue(); // Remove the droplet once reached
                }
            }

           // If we've exceeded the max number of droplets or no droplets remain
            if (dropletCounter > maxDroplets || droplets.Count == 0)
            {
                Debug.Log("Exceeded max droplets or no droplets remain. Transitioning to IdleState.");
                StopAllCoroutines();
                isFollowingDroplets = false;
                yield break; // Exit the coroutine
            }
        }
    }

    #region
    /// <summary>
    /// Adjusts the seekWeight and avoidWeight dynamically based on how near/far both the player and obstacles are
    /// </summary>
    /// <param name="distanceToTarget">the distance we are from our target in order for us to determine how much we should Lerp between our max and 
    /// min values divided by how accurate we want our AI to seek distance</param>     
    #endregion
    private void AdjustWeightsDynamically(float distanceToTarget)
    {
        seekWeight = Mathf.Lerp(2.0f, 0.5f, Mathf.Clamp01(distanceToTarget / distanceClamp)); // Closer = lower seek
        avoidWeight = Mathf.Lerp(0.5f, 2.0f, Mathf.Clamp01(distanceToTarget / distanceClamp)); // Closer = higher avoid
    }

    #region
    /// <summary>
    /// Calculates the force of which our enemy AI steers itself towards our desired target multiped by our weights  
    /// </summary>
    /// <param name="targetPosition">The target we want our AI to chase after</param>
    /// <returns>the sum of the seekForce and avoidForce in order to determine the total force it should steer itself towards our target</returns> 
    #endregion
    private Vector2 CalculateSteeringForce(Vector2 targetPosition)
    {
        Vector2 seekForce = Seek(targetPosition) * seekWeight;
        Vector2 avoidForce = AvoidObstacles() * avoidWeight;

        return seekForce + avoidForce;
    }

    private Vector2 Seek(Vector2 targetPosition)
    {
        Vector2 desiredVelocity = (targetPosition - (Vector2)rb.position).normalized * speed;
        return desiredVelocity - rb.velocity;
    }

    private Vector2 AvoidObstacles()
    {
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(rb.position, obstacleDetectionRadius, obstacleLayer);

        Vector2 avoidanceForce = Vector2.zero;
        foreach (var obstacle in obstacles)
        {
            Vector2 directionAway = (rb.position - (Vector2)obstacle.transform.position).normalized;
            avoidanceForce += directionAway / Vector2.Distance(rb.position, obstacle.transform.position);
        }

        return avoidanceForce.normalized * speed;
    }

    private void ApplySteering(Vector2 force)
    {
        rb.velocity = Vector2.ClampMagnitude(rb.velocity + force * Time.deltaTime, speed);
    }

    private bool isPlayerNear()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, playerRadius, playerMask);
    }

    private void OnDrawGizmos()
    {
        if(showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, obstacleDetectionRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(enemyTransform.position, playerRadius);

            Gizmos.color = Color.green;
            if (playerTransform != null)
            {
                Gizmos.DrawLine(rb.position, playerTransform.position);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastKnownPosition, 0.2f);

            foreach (var droplet in droplets)
            {
                Gizmos.DrawWireSphere(droplet, 0.2f);
            }
        }
    }
}