using Pathfinding;
using UnityEngine;

public class IdleState : State
{

    #region General
    [Header("General")]
    public bool showGizmos;
    #endregion

    #region States to transition to
    [Header("States to transition to")]
    public State chaseState;
    public State attackState;
    #endregion

    #region Tracking the Player
    [Header("Tracking the Player")]
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject point;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private float playerRadius;
    [SerializeField] private int rayCount;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleLayer;
    private GridGraph grid;
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    public AIPath aiPath;
    public bool isMoving = false;
    #endregion

    void Start()
    {
        grid = AstarPath.active.data.gridGraph;
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiPath = enemy.GetComponent<AIPath>();
        pointToGoTowards();
    }

    public override State RunCurrentState()
    {
        if (!aiPath.pathPending && aiPath.reachedDestination && !isMoving)
        {
            pointToGoTowards();
        }
        
        if (isPlayerNear())
        {
            showGizmos = false;
            if(enemy.gameObject.tag == "StaticEnemy")
            {
                return attackState;
            }
            else
            {
                return chaseState; 
            }
        }
        else
        {
            showGizmos = true;
        }

        

        return this;
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

    private GameObject pointToGoTowards()
    {
        // Create a new point with a trigger collider
        if (point != null) {Destroy(point);} // Destroy the old point if it exists

        Vector3 randomPoint = PickRandomPoint(); // Get a valid point
        point = new GameObject("Point");
        point.transform.position = randomPoint;
        CircleCollider2D cirCollider = point.AddComponent<CircleCollider2D>(); // Add a collider to the point
        cirCollider.isTrigger = true; // Set collider as trigger
        aiPath.destination = randomPoint; // Set the new destination for the AI

        Debug.Log($"🎯 AI destination set to: {randomPoint}");
        point.tag = "Destination";

        return point; // Return the new GameObject (destination point)
    }

  private Vector3 PickRandomPoint()
    {
        Vector3 randomPoint = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.depth)); // Random point in grid
        randomPoint.z = 0; // Ensure it's 2D movement
        randomPoint += transform.position; // Offset from AI's position

        // Ensure the point is on a walkable node and valid
        GraphNode node = AstarPath.active.GetNearest(randomPoint).node;

        if (node != null && node.Walkable)
        {
            Debug.Log($"Picked valid point: {randomPoint}.");
            return (Vector3)node.position;
        }
        else
        {
            Debug.LogWarning("⚠️ Picked an unwalkable point, retrying...");
            return PickRandomPoint(); // Retry if the point is invalid
        }
    }


    private void OnDrawGizmos()
    {   
        if(showGizmos)
        {
            Gizmos.color = Color.blue; // Ray color for visibility

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