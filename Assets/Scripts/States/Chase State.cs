using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    /*TODO:
     FIX THE DROPLET SYSTEM WITH THIS: 
     * When the player exists the enemiy's radius the player drop an invisble gameobject by the name of droplet. 
     * Afterwards, the enemy will move towards the droplet and see if the player exists at it. 
     * After doing this 2-3 times (AKA spawning 2-3 droplets) the enemy will give up and enter the wander state. 
     */

    #region References
    [Header("Reference")]
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
    private Vector2 lastKnownPosition;
    #endregion

    public void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        enemyTransform = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Transform>();
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
            StartCoroutine(FindLastPosition(duration)); // Update last known position when player is visible
        }
        else
        {
            Debug.Log("Player not visible, moving to last known position.");
            StopCoroutine(FindLastPosition(duration));
        }

        Debug.Log(isPlayerNear());

        // Dynamically adjust weights based on proximity to the last known position
        AdjustWeightsDynamically(Vector2.Distance(rb.position, lastKnownPosition));

        // Perform context-oriented steering to chase the player or move to the last known position
        Vector2 targetPosition = isPlayerNear() ? playerTransform.position : lastKnownPosition;
        Vector2 steeringForce = CalculateSteeringForce(targetPosition);
        ApplySteering(steeringForce);

        Debug.Log("Chasing player or moving to last known position.");
        return this; // Remain in ChaseState
    }

    private void AdjustWeightsDynamically(float distanceToTarget)
    {
        seekWeight = Mathf.Lerp(2.0f, 0.5f, Mathf.Clamp01(distanceToTarget / distanceClamp)); // Closer = lower seek
        avoidWeight = Mathf.Lerp(0.5f, 2.0f, Mathf.Clamp01(distanceToTarget / distanceClamp)); // Closer = higher avoid
    }

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

    private IEnumerator FindLastPosition(float duration)
    {
        while (true)
        {
            yield return new WaitForSeconds(duration);
            lastKnownPosition = playerTransform.position;
        }
    }

    private bool isPlayerNear()
    {
        return Physics2D.OverlapCircle(enemyTransform.position, playerRadius, playerMask);
    }

    private void OnDrawGizmos()
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
    }
}