using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : State
{
	//TODO: Maybe find a way to pool the waypoints the enemies have to go towards
	//TODO: Find a way to fix the "bunching" issue that happen when AI either has the same point or on top of each other
	//TODO: Fix the freezing issue that happens with random Distance Factor
	//TODO: Have it so it can detect the player if it attacked <-- On Hold: Might have to tinker around with the State Manager
	#region General
	[Header("General")]
	[SerializeField] private GameObject enemy;
	[SerializeField] private Transform enemyTransform;
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
	private readonly int maxRetries = 10;
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
		fov = enemy.GetComponent<FOV>();
		enemyTransform = enemy.transform;
		grid = AstarPath.active.data.gridGraph;
		aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
		aiPath = enemy.GetComponent<AIPath>();
		aiLerp = enemy.GetComponent<AILerp>();
		GameObject newPoint = pointToGoTowards();
		aiDestinationSetter.target = newPoint.transform;
	}

	public override State RunCurrentState()
	{
		aiLerp.speed = wanderSpeed;

		if (!aiPath.pathPending && aiPath.reachedDestination)
		{
			GameObject newPoint = pointToGoTowards();
			aiDestinationSetter.target = newPoint.transform;

			aiPath.canMove = true;
			aiPath.SearchPath();
		}

		if (IsAIStuck())
		{
			TeleportToRandomLocation();
		}

		if (fov.canSeePlayer)
		{
			return pursuitState;
		}

		return this;
	}

	/// <summary>
	/// Creates a new GameObject representing a destination point for the enemy to move towards.
	/// </summary>
	/// <returns> The destination point</returns>
	private GameObject pointToGoTowards()
	{
		GameObject newPoint = new GameObject("Point_" + enemy.GetInstanceID());
		newPoint.transform.position = PickRandomPoint();
		CircleCollider2D cirCollider = newPoint.AddComponent<CircleCollider2D>();
		cirCollider.isTrigger = true;
		newPoint.tag = "Destination";
		return newPoint;
	}

	/// <summary>
	/// Picks a random point within the grid that is walkable and sufficiently far from the last position.
	/// </summary>
	/// <returns> A random point within the grid if that node is walkable, zero otherwiese</returns>
	private Vector3 PickRandomPoint()
	{
		Vector3 randomPoint = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.depth));
		randomPoint.z = 0;
		randomPoint += transform.position;
		Vector3 lastPosition = Vector3.zero;
		int retries = 0;

		float randomMaxDistance = minDistanceBetweenPoints * Random.Range(0.5f, randomDistanceFactor);

		while (Vector3.Distance(randomPoint, lastPosition) < minDistanceBetweenPoints)
		{
			randomPoint = new Vector3(Random.Range(0, grid.width), Random.Range(0, grid.depth));
			randomPoint.z = 0;
			randomPoint += transform.position;
			if (retries >= maxRetries)
			{
				break;
			}
			retries++;
		}

		GraphNode node = AstarPath.active.GetNearest(randomPoint).node;

		if (node != null && node.Walkable)
		{
			lastPosition = randomPoint;
			return (Vector3)node.position;
		}
		else
		{
			return Vector3.zero; // Return zero vector if the point is invalid
		}
	}

	/// <summary>
	/// Checks if the AI is stuck by comparing its current position to its last known position.
	/// </summary>
	/// <returns> True if time of last movement exceeds our stuck time thershold, false otherwise</returns>
	private bool IsAIStuck()
	{
		if (Vector3.Distance(aiPath.transform.position, lastKnownPosition) < 0.001f)
		{
			timeSinceLastMovement += Time.deltaTime;
		}
		else
		{
			timeSinceLastMovement = 0f;
			lastKnownPosition = aiPath.transform.position;
		}

		return timeSinceLastMovement >= stuckTimeThreshold;
	}

	/// <summary>
	/// Teleports the AI to a random valid location if it is stuck.
	/// </summary>
	private void TeleportToRandomLocation()
	{
		Vector3 randomPoint = PickRandomPoint();
		if (randomPoint != Vector3.zero)
		{
			aiPath.Teleport(randomPoint);
			aiPath.destination = randomPoint;
			timeSinceLastMovement = 0f;
		}
	}


}
