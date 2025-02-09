using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOV : MonoBehaviour
{
    //TODO:
    //FOV keeps following with the rotation of the rigidbody, should we make it its own object?

    [SerializeField] private float fov = 90f; // Field of view in degrees
    [SerializeField] private float distance = 5f; // Max raycast distance
    [SerializeField] private int rayCount = 10; // Number of FOV rays
    [SerializeField] private int smallerRaysCount = 12; // Rays around the enemy
    [SerializeField] private float smallerRayDistance = 2f; // Distance for smaller rays

    private Rigidbody2D rb;
    [SerializeField] private GameObject player;
    [SerializeField] private LayerMask layerMask;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (rb == null || player == null) return;

        // Get the enemy's current facing angle
        float facingAngle = transform.eulerAngles.z + 90f;

        // Calculate the starting angle for the FOV
        float angleStep = fov / (rayCount - 1);
        float startAngle = facingAngle - (fov / 2);

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
                Debug.Log("Player detected in FOV!");
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
                Debug.Log("Player detected near enemy!");
            }
        }
    }
}
