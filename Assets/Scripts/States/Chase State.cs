using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    /*TODO:
      Find a way to make it go one droplet at a time
     */
    //THE Y SCALE FOR THE WALL CANNOT BE SMALLER THAN 10

    #region General
    [Header("General")]
    [SerializeField] private bool showGizmos = true;
    #endregion

    #region References
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator chaseAnimator;
    [SerializeField] private Animation chaseAnimation;
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
    [SerializeField] private float playerRadius;
    private float attackRadius;
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
    [SerializeField] private float dropletDiscard;
    private Queue<GameObject> droplets = new Queue<GameObject>(); // Queue of GameObjects (droplets)
    #endregion

    public void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        attackRadius = playerRadius / 1.5f;
    }

    #region 
    /// <summary>
    /// A function used to run whatever behavior the state is executing 
    /// (basically acts as our void Update in our individual states)
    /// </summary>
    /// <returns>the state we're currently in or another state</returns>
    #endregion 
    public override State RunCurrentState()
    {
        Debug.Log("Chase state is activated");
        if (playerTransform == null)
        {
            Debug.LogError("Player not assigned, transitioning to IdleState.");
            return idleState;
        }

        if (isPlayerNear())
        {
            lastKnownPosition = playerTransform.position;
            dropletCounter = 0;
        }
        else if (!isFollowingDroplets && !isPlayerNear())
        {
            StartCoroutine(FollowDroplet(lastKnownPosition, duration));
            isFollowingDroplets = true;
        }

        if (dropletCounter > maxDroplets)
        {
            Reset();
            return idleState;
        }

        // Dynamically adjust weights based on proximity to the last known position
        AdjustWeightsDynamically(Vector2.Distance(rb.position, lastKnownPosition));

        // Perform context-oriented steering to chase the player or move to the last known position
        GameObject targetDroplet = droplets.Count > 0 ? droplets.Peek() : null;
        Vector2 targetPosition = targetDroplet != null ? targetDroplet.transform.position : playerTransform.position;
        Vector2 steeringForce = CalculateSteeringForce(targetPosition);
        ApplySteering(steeringForce);

        Debug.Log("Chasing player or moving to last known position.");

        return this; // Remain in ChaseState
    }

    public IEnumerator FollowDroplet(Vector2 position, float duration)
    {
        if (droplets.Count == 0)
        {
            droplets.Enqueue(InstantiateDroplet(position)); // Create the first droplet
            dropletCounter = 1;
        }

        while (true)
        {
            yield return new WaitForSeconds(duration);

            if (droplets.Count > 0)
            {
                GameObject targetDroplet = droplets.Peek();

                // Check if the droplet is too close or the enemy has reached it
                if (Vector2.Distance(targetDroplet.transform.position, rb.position) < dropletDiscard || 
                    (Vector2)targetDroplet.transform.position == rb.position)
                {
                    droplets.Dequeue(); // Remove the droplet
                    dropletCounter++;   // Increment the counter

                    // Destroy the old droplet
                    Destroy(targetDroplet);

                    // Create a new droplet and enqueue it
                    Vector2 newDropletPosition = CalculateNextDropPos();
                    droplets.Enqueue(InstantiateDroplet(newDropletPosition));
                }
            }
            
            if (dropletCounter > maxDroplets || droplets.Count == 0)
            {
                Debug.Log("Exceeded max droplets or no droplets remain. Transitioning to IdleState.");
                StopAllCoroutines();
                isFollowingDroplets = false;
                yield break; // Exit the coroutine
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    #region 
    /// <summary>
    /// Guesses where the player is by adding the player's position by random point inside a unit circle multiplied by a float value
    /// </summary>
    /// <returns> The sum of the player position and the multiplied unit circle value </returns>
    #endregion
    private Vector2 CalculateNextDropPos()
    {
        // Direction vector towards the player
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;

        // New position in the direction of the player within the detection radius
        Vector2 proposedPosition = rb.position + directionToPlayer * playerRadius;

        // Clamp the position within the detection radius
        if (Vector2.Distance(rb.position, proposedPosition) > playerRadius)
        {
            proposedPosition = rb.position + directionToPlayer * playerRadius;
        }

        return proposedPosition;
    }

    private bool isDropletNearWall(Vector2 dropletPos)
    {
        Collider2D[] dropletObstacles =  Physics2D.OverlapCircleAll(dropletPos, obstacleDetectionRadius, obstacleLayer);
        return dropletObstacles.Length > 0;
    }

    private GameObject InstantiateDroplet(Vector2 position)
    {
        GameObject droplet = new GameObject("Droplet");
        droplet.transform.position = position;
        return droplet;
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
        Vector2 avoidForce = AvoidObstacles()  * avoidWeight;

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
            // Calculate the direction away from the obstacle
            Vector2 directionAway = (rb.position - (Vector2)obstacle.transform.position).normalized;

            // Find the perpendicular direction (90 degrees to the directionAway vector)
            Vector2 perpendicular = new Vector2(-directionAway.y, directionAway.x);

            // Weight the perpendicular force more strongly when close to the obstacle
            float distanceToObstacle = Vector2.Distance(rb.position, obstacle.transform.position);
            float weight = Mathf.Clamp(1 / distanceToObstacle, 0, 1); // Clamp weight to avoid excessive forces
            avoidanceForce += perpendicular * weight;
        }

        // Gradually blend the avoidance force with the current velocity
        avoidanceForce = Vector2.Lerp(rb.velocity.normalized, avoidanceForce.normalized, 0.1f);

        // Return a scaled avoidance force
        return avoidanceForce * speed * 1.5f; // Reduced amplification for smoother steering
        }



    private void ApplySteering(Vector2 force)
    {
        // Smoothly apply steering force to the velocity
        Vector2 desiredVelocity = rb.velocity + force * Time.deltaTime;

        // Gradually clamp the velocity to avoid abrupt changes
        rb.velocity = Vector2.Lerp(rb.velocity, Vector2.ClampMagnitude(desiredVelocity, speed), 0.5f);
    }


    #region 
    /// <summary>
    /// Resets Everything in the ChaseState
    /// </summary>
    #endregion
    private void Reset()
    {
        rb.velocity = Vector2.zero;
        StopAllCoroutines();
        foreach (var droplet in droplets)
        {
            Destroy(droplet); // Destroy all droplets to avoid memory leak
        }
        droplets.Clear();
        isFollowingDroplets = false;
        dropletCounter = 0;
    }

    private bool isPlayerNear()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, playerRadius, playerMask);
    }

    private bool isPlayerInAttackRange()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, attackRadius, playerMask);
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, obstacleDetectionRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(enemyTransform.position, playerRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(enemyTransform.position, attackRadius);

            Gizmos.color = Color.green;
            if (playerTransform != null)
            {
                Gizmos.DrawLine(rb.position, playerTransform.position);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastKnownPosition, 0.2f);

            foreach (var droplet in droplets)
            {
                Gizmos.DrawWireSphere(droplet.transform.position, 0.2f);

                if(isDropletNearWall(droplet.transform.position))
                {
                    Gizmos.DrawWireSphere(droplet.transform.position, obstacleDetectionRadius);
                }
                
            }
        }
    }
}
