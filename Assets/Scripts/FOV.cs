using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class FOV : MonoBehaviour
{
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
	private AILerp aiLerp;

	// Buffer array for RaycastNonAlloc results
	private RaycastHit2D[] hitsBuffer = new RaycastHit2D[10];
	void Start()
	{
		aiLerp = gameObject.GetComponent<AILerp>();
		rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D
		player = GameObject.FindGameObjectWithTag("Player");
		aiPath = GetComponent<AIPath>();
	}

	void Update()
	{
		if (rb == null || player == null || aiPath == null) return;

		// Get facing angle of AI
		float facingAngle = GetFacingAngle();

		// Cast FOV rays
		if (CheckFOV(facingAngle)) return;

		// Cast smaller rays around the enemy
		CheckSmallerRays();
	}

	private float GetFacingAngle()
	{
		// Get direction towards the AI's steering target (next path point)
		Vector2 nextWaypointDirection = ((Vector2)aiPath.steeringTarget - (Vector2)transform.position).normalized;

		// Convert direction to angle
		return Mathf.Atan2(nextWaypointDirection.y, nextWaypointDirection.x) * Mathf.Rad2Deg;
	}

	private bool CheckFOV(float facingAngle)
	{
		// Calculate the starting angle for the FOV
		float angleStep = fov / (rayCount - 1);
		float startAngle = facingAngle - (fov / 2);

		canSeePlayer = false;

		// Cast the FOV rays
		for (int i = 0; i < rayCount; i++)
		{
			float angle = startAngle + (i * angleStep);
			Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

			// Perform the raycast and store results in the hitsBuffer
			int hitCount = Physics2D.RaycastNonAlloc(rb.position, direction, hitsBuffer, distance, layerMask);

			// Visualize the FOV rays (Red)
			Debug.DrawRay(rb.position, direction * distance, Color.red);

			// Iterate through all hits
			for (int j = 0; j < hitCount; j++)
			{
				RaycastHit2D hit = hitsBuffer[j];
				if (hit.collider != null && hit.collider.gameObject == player)
				{
					canSeePlayer = true;
					return true; // Player detected, break out of FOV check
				}
			}
		}

		return false; // No player detected in FOV
	}

	private void CheckSmallerRays()
	{
		// Cast smaller rays around the enemy if player is not already detected
		float angleIncrement = 360f / smallerRaysCount;

		for (int i = 0; i < smallerRaysCount; i++)
		{
			float angle = i * angleIncrement;
			Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

			// Perform the raycast and store results in the hitsBuffer
			int hitCount = Physics2D.RaycastNonAlloc(transform.position, direction, hitsBuffer, smallerRayDistance, layerMask);

			// Visualize the smaller rays (Blue)
			Debug.DrawRay(transform.position, direction * smallerRayDistance, Color.blue);

			// Iterate through all hits
			for (int j = 0; j < hitCount; j++)
			{
				RaycastHit2D smallHit = hitsBuffer[j];
				if (smallHit.collider != null && smallHit.collider.gameObject == player)
				{
					canSeePlayer = true;
					return; // Player detected near enemy, break out of smaller ray check
				}
			}
		}
	}
}
