using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    //TODO: Make it so we dectect player using Raycasts in 8 caridnal direction
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
    [SerializeField] private int rayCount;
    [SerializeField] private bool isFollowingDroplets;
    [SerializeField] private float dropletDiscard;
    [SerializeField] private float avoidenceLerp;
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

    #region
    /// <summary>
    /// Takes the last known position of the player and creates droplets every set amount of seconds for the enemy got to and collect , if the player is near the latest droplet it will go back to chasing the player
    /// </summary>
    /// <param name="position">the last known position of the player</param>
    /// <param name="duration">the amount of time it should wait to create a new droplet</param>
    #endregion
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
    /// Calculates the distance between the player and the enemy and gets a new droplet position around the circumstance of the playerRadius
    /// </summary>
    /// <returns> The position where the droplet should be on the playerRadius based on the direction of the player </returns>
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

    #region 
    /// <summary>
    /// Checks if the droplet is near a wall
    /// </summary>
    /// <param name="dropletPos"> a reference to the position of the droplet in order to place the center of the OverlapCircleAll</param>
    /// <returns></returns>
    #endregion 
    private bool isDropletNearWall(Vector2 dropletPos)
    {
        Collider2D[] dropletObstacles =  Physics2D.OverlapCircleAll(dropletPos, obstacleDetectionRadius, obstacleLayer);
        return dropletObstacles.Length > 0;
    }

    #region 
    /// <summary>
    /// Insanities a droplet at the last known position of the player
    /// </summary>
    /// <param name="position">the last known position of the player</param>
    /// <returns></returns>
    #endregion
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

    #region 
    /// <summary>
    /// Calculates if there are any obstacles in the way of your enemy and adjusts the force of which our enemy is steering away from the obstacle accordingly 
    /// </summary>
    /// <returns>The scaled avoidance force</returns>
    #endregion
    private Vector2 AvoidObstacles()
    {
        Vector2 avoidanceForce = Vector2.zero;

        // Get all obstacles in range using OverlapCircle
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(rb.position, obstacleDetectionRadius, obstacleLayer);

        foreach (var obstacle in obstacles)
        {
            // Check if the obstacle is blocking the path using raycasting
            Vector2 directionToObstacle = ((Vector2)obstacle.transform.position - rb.position).normalized;

            // Cast a ray from the enemy to the obstacle to check for line-of-sight
            RaycastHit2D hit = Physics2D.Raycast(rb.position, directionToObstacle, obstacleDetectionRadius, obstacleLayer);

            if (hit.collider != null && hit.collider == obstacle)
            {
                // Calculate the direction away from the obstacle
                Vector2 directionAway = (rb.position - (Vector2)obstacle.transform.position).normalized;

                // Calculate the perpendicular direction (90 degrees to the directionAway vector)
                Vector2 perpendicular = new Vector2(-directionAway.y, directionAway.x);

                // Use the distance to obstacle to scale the avoidance force more strongly as we get closer
                float distanceToObstacle = Vector2.Distance(rb.position, obstacle.transform.position);
                float weight = Mathf.Clamp(1 / distanceToObstacle, 0, 1); // Clamped weight to prevent excessive forces

                // Apply the avoidance force in the perpendicular direction, scaled by the distance
                avoidanceForce += perpendicular * weight;
            }
        }

        // Check if any droplets are visible and avoid them if not blocked by an obstacle
        foreach (var droplet in droplets)
        {
            // Cast a ray to the droplet position to see if it's visible (i.e., no obstacles in between)
            RaycastHit2D hit = Physics2D.Raycast(rb.position, ((Vector2)droplet.transform.position - rb.position).normalized, playerRadius, obstacleLayer);

            // If the ray hits an obstacle or does not hit the droplet, it's not visible
            if (hit.collider == null || hit.collider.gameObject != droplet)
            {
                // Avoid the droplet since it's not visible to the enemy
                Vector2 directionAway = (rb.position - (Vector2)droplet.transform.position).normalized;
                Vector2 perpendicular = new Vector2(-directionAway.y, directionAway.x);

                // Avoidance force, scaled by the distance to the droplet
                float distanceToDroplet = Vector2.Distance(rb.position, droplet.transform.position);
                float weight = Mathf.Clamp(1 / distanceToDroplet, 0, 1); // Avoidance weight based on distance

                avoidanceForce += perpendicular * weight;
            }
        }

        // Gradually blend the avoidance force with the current velocity
        avoidanceForce = Vector2.Lerp(rb.velocity.normalized, avoidanceForce.normalized, avoidenceLerp);

        // Return the final avoidance force, scaled to the enemy's speed for smoother movement
        return avoidanceForce * speed * 1.5f;
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
        // Calculate the directions dynamically based on rayCount
        Vector2[] directions = new Vector2[rayCount];
        float angleStep = 360f / rayCount; // Evenly space the rays in a circle

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            directions[i] = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)).normalized;
        }

        // Cast the rays and check for a player hit, ignoring rays that hit obstacles
        foreach (Vector2 direction in directions)
        {            

            // Cast a ray to check if it hits any obstacle
            RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, direction, playerRadius, obstacleLayer);

            // If the ray hits an obstacle, stop the raycast and ignore any further detection
            if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                continue; // Skip this ray if it hits an obstacle
            }

            // Cast the ray again to check for the player, ensuring the ray is valid
            RaycastHit2D playerHit = Physics2D.Raycast(enemyTransform.position, direction, playerRadius, playerMask);

            // If the ray hits the player directly (without hitting an obstacle), return true
            if (playerHit.collider != null && ((1 << playerHit.collider.gameObject.layer) & playerMask) != 0)
            {
                return true; // Player detected without obstacles
            }
            
        }

        return false; // No clear line of sight to the player
    }

    private bool isPlayerInAttackRange()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, attackRadius, playerMask);
    }

    #region 
    /// <summary>
    /// if showGizmos is true shows all important aspects of the script visually
    /// </summary> <summary>
    #endregion
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, obstacleDetectionRadius);

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

                if (isDropletNearWall(droplet.transform.position))
                {
                    Gizmos.DrawWireSphere(droplet.transform.position, obstacleDetectionRadius);
                }

            }

            // Debug the raycasts to visualize their dynamic lengths
            Gizmos.color = Color.yellow; // Ray color for visibility

            // Calculate the directions dynamically based on rayCount
            Vector2[] directions = new Vector2[rayCount];
            float angleStep = 360f / rayCount; // Evenly space the rays in a circle

            // Populate the directions array with evenly spaced angles
            for (int i = 0; i < rayCount; i++)
            {
                float angle = i * angleStep;
                directions[i] = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)).normalized;
            }

            // Draw rays in a circle from the enemy's position
            foreach (Vector2 direction in directions)
            {
                // Cast a ray to check if it hits any obstacle
                RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, direction, playerRadius, obstacleLayer);

                // Calculate the dynamic ray length
                float rayLength = playerRadius;
                if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                {
                    // If an obstacle is hit, reduce the ray length based on proximity to the obstacle
                    float distanceToObstacle = Vector2.Distance(enemyTransform.position, hit.point);
                    rayLength = Mathf.Clamp(playerRadius - distanceToObstacle, 0f, playerRadius);
                }

                // Draw the dynamic-length ray
                Gizmos.DrawLine(enemyTransform.position, (Vector2)enemyTransform.position + direction * rayLength);
            }
        }
    }
}
