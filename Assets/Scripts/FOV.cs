using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class FOV : MonoBehaviour
{
    //TODO: Add Knockback to player
    //TODO: Add Spawner 

    [SerializeField] private float fov = 90f; // Field of view in degrees
    public float distance = 5f; // Max raycast distance
    [SerializeField] private int rayCount = 10; // Number of FOV rays
    [SerializeField] private int smallerRaysCount = 12; // Rays around the enemy
    [SerializeField] private float smallerRayDistance = 2f; // Distance for smaller rays
    [SerializeField] private AIPath aiPath;

    private Rigidbody2D rb;
    [SerializeField] private GameObject player;
    [SerializeField] private LayerMask layerMask;
    public bool canSeePlayer = false;

    [SerializeField] private float pursuitSpeed;
    [SerializeField] private float wanderSpeed;
    private AILerp aiLerp;

    void Start()
    {
        aiLerp = gameObject.GetComponent<AILerp>();
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D
        player = GameObject.FindGameObjectWithTag("Player");
        aiPath = GetComponent<AIPath>();
    }

    void Update()
    {
        if (rb == null || player == null) return;

        float facingAngle;
        if (aiPath != null)
        {
            // Get direction towards the AI's steering target (next path point)
            Vector2 nextWaypointDirection = ((Vector2)aiPath.steeringTarget - (Vector2)transform.position).normalized;

            // Convert direction to angle
            facingAngle = Mathf.Atan2(nextWaypointDirection.y, nextWaypointDirection.x) * Mathf.Rad2Deg;
        }
        else
        {
            // Default to current rotation if AIPath is missing
            facingAngle = transform.eulerAngles.z;
        }

        // Calculate the starting angle for the FOV
        float angleStep = fov / (rayCount - 1);
        float startAngle = facingAngle - (fov / 2);
        
        canSeePlayer = false;

        // Cast the FOV rays
        for (int i = 0; i < rayCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, distance, layerMask);

            // Visualize the FOV rays (Red)
            Debug.DrawRay(rb.position, direction * distance, Color.red);

            // If the ray hits the player
            if (hit.collider != null && hit.collider.gameObject == player)
            {
                canSeePlayer = true;
                Debug.Log("Player detected in FOV!");
                break;
            }


        }
        
        // Cast smaller rays around the enemy
        float angleIncrement = 360f / smallerRaysCount;

        for (int i = 0; i < smallerRaysCount; i++)
        {
            float angle = i * angleIncrement;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            RaycastHit2D smallHit = Physics2D.Raycast(transform.position, direction, smallerRayDistance, layerMask);

            // Visualize the smaller rays (Blue)
            Debug.DrawRay(transform.position, direction * smallerRayDistance, Color.blue);

            // If the ray hits the player
            if (smallHit.collider != null && smallHit.collider.gameObject == player)
            {
                canSeePlayer = true;
                Debug.Log("Player detected near enemy!");
                break;
            }
        }

        if (canSeePlayer)
        {
            aiLerp.speed = pursuitSpeed;
        }
        else
        {
            aiLerp.speed = wanderSpeed;
        }

    }

}