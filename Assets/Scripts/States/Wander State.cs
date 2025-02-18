using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : State
{
    //TODO: Have each enemy have their own unique points so that we can have multiple enemies moving at once
    #region General
    [Header("General")]
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform enemyTransform;
    public bool showGizmos;
    #endregion

    #region States to transition to
    [Header("States to transition to")]
    public State pursuitState;
    #endregion

    #region AStarGrid and Scripts
    [Header("AStarGrid and Scripts")]
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private AILerp aiLerp;
    private GridGraph grid;
    #endregion


    #region Tracking the Player
    [Header("Traversing towards the point")]
    [SerializeField] private GameObject point;
    private readonly int maxRetries;
    [SerializeField][Range(1f, 40f)] private float minDistanceBetweenPoints;
    [SerializeField][Range(0.5f, 2.0f)] private float randomDistanceFactor = 1.0f;
    [SerializeField] private float wanderSpeed;
    [SerializeField] private float stuckTimeThreshold = 3.0f; // Time threshold before considering the AI stuck (in seconds)
    [SerializeField] private float timeSinceLastMovement = 0.0f; // Timer to track how long since the AI last moved
    [SerializeField] private Vector3 lastKnownPosition;

    public FOV fov;
    #endregion

    #region Animation
    [Header("Animation")]
    [SerializeField] private Animator animatior;
    [SerializeField] private Animation animation;
    #endregion


    private void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        //aiLerp = enemy.GetComponent<AILerp>();
        fov = enemy.GetComponent<FOV>();
        enemyTransform = enemy.transform;
        grid = AstarPath.active.data.gridGraph;
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiPath = enemy.GetComponent<AIPath>();
        aiLerp = enemy.GetComponent<AILerp>();
        aiDestinationSetter.target = pointToGoTowards().transform;
    }

    public override State RunCurrentState()
    {
        aiLerp.speed = wanderSpeed;
        if (!aiPath.pathPending && aiPath.reachedDestination)
        {
            aiDestinationSetter.target = pointToGoTowards().transform;
        }
        // Check if the AI is stuck (not moving for too long)
        if (IsAIStuck())
        {
            Debug.Log("AI is stuck, teleporting to a random location!");
            TeleportToRandomLocation(); // Teleport AI to a random position
        }
        if (fov.canSeePlayer)
        {
            return pursuitState;
        }
        return this;
    }

    /// <summary>
    /// Creates a point the enemy can traverse towards
    /// </summary>
    /// <returns>The point that the enemy traverses towards</returns>
    private GameObject pointToGoTowards()
    {
        // Create a new point with a trigger collider
        if (point != null) { Destroy(point); } // Destroy the old point if it exists

        Vector3 randomPoint = PickRandomPoint(); // Get a valid point
        point = new GameObject("Point");
        point.transform.position = randomPoint;
        CircleCollider2D cirCollider = point.AddComponent<CircleCollider2D>(); // Add a collider to the point
        cirCollider.isTrigger = true; // Set collider as trigger
        //aiPath.destination = randomPoint; // Set the new destination for the AI

        Debug.Log($"AI destination set to: {randomPoint}");
        point.tag = "Destination";

        return point; // Return the new GameObject (destination point)
    }

    /// <summary>
    /// Picks a random spot on the map in order for our point 
    /// </summary>
    /// <returns>a random spot the pointToGoTowards object can use to set it's position</returns>
    private Vector3 PickRandomPoint()
    {
        Vector3 randomPoint = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.depth)); // Random point in grid
        randomPoint.z = 0; // Ensure it's 2D movement
        randomPoint += transform.position; // Offset from AI's position
        Vector3 lastPosition = Vector3.zero;
        int retries = 0;

        float randomMaxDistance = minDistanceBetweenPoints * Random.Range(0.5f, randomDistanceFactor);

        // Ensure the new point is sufficiently far from the last one
        while (Vector3.Distance(randomPoint, lastPosition) < minDistanceBetweenPoints)
        {
            // Keep generating a new point until it's far enough from the last point
            randomPoint = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.depth));
            randomPoint.z = 0;
            randomPoint += transform.position;
            if (retries >= maxRetries)
            {
                Debug.LogWarning("⚠️ Maximum retries reached. Using the last valid point.");
                break; // Exit loop if max retries are reached
            }
        }

        // Ensure the point is on a walkable node and valid
        GraphNode node = AstarPath.active.GetNearest(randomPoint).node;

        if (node != null && node.Walkable)
        {
            lastPosition = randomPoint; // Update last valid position
            return (Vector3)node.position;
        }
        else
        {
            Debug.LogWarning("⚠️ Picked an unwalkable point, retrying...");
            return PickRandomPoint(); // Retry if the point is invalid
        }

    }

    /// <summary>
    /// Checks if the AI is stuck in place based on the where and when it moves last, and starts a timer if it's stuck
    /// </summary>
    /// <returns> if our enemy has been stuck there too long based on our timer</returns>
    private bool IsAIStuck()
    {
        // Calculate how much time has passed since the last movement
        if (Vector3.Distance(aiPath.transform.position, lastKnownPosition) < 0.1f)
        {
            timeSinceLastMovement += Time.deltaTime; // Increase the timer if AI has not moved
        }
        else
        {
            timeSinceLastMovement = 0f; // Reset the timer if AI has moved
            lastKnownPosition = aiPath.transform.position; // Update last known position
        }

        // If the AI has been stuck for longer than the threshold, consider it stuck
        return timeSinceLastMovement >= stuckTimeThreshold;
    }

    /// <summary>
    /// Teleport the AI to a random position on the map
    /// </summary>
    private void TeleportToRandomLocation()
    {
        Vector3 randomPoint = PickRandomPoint(); // Pick a random valid point
        aiPath.Teleport(randomPoint); // Teleport AI to the random position
        aiPath.destination = randomPoint; // Set the destination to the new random point
        timeSinceLastMovement = 0f; // Reset the stuck timer
    }
}
