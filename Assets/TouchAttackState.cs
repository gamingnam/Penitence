using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TouchAttackState : State
{
    //TODO: Attack is happening too fast 
    #region General
    [Header("General")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject player;
    [SerializeField] private bool hasTouchAttacked;
    [SerializeField] private float coolDownSeconds;
    [SerializeField] private float coolLungeSecond;
    [SerializeField] private float lungeStrength;
    [SerializeField] private PlayerScript ps;
    #endregion

    #region AStarGrid and Scripts
    [Header("AStarGrid and Scripts")]
    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private AILerp aiLerp;
    #endregion

    #region States to Transition to
    [Header("States to Transition to")]
    [SerializeField] private State pursuitState;
    #endregion

    void Start()
    {
        
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiPath = enemy.GetComponent<AIPath>();
        aiLerp = enemy.GetComponent<AILerp>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        ps = player.GetComponent<PlayerScript>();
        rb.AddForce(Vector2.zero, ForceMode2D.Impulse);
    }

    public override State RunCurrentState()
    {
        if (!hasTouchAttacked)
        {
            aiDestinationSetter.enabled = false;
            aiPath.enabled = false;
            StartCoroutine(TouchAttack());
        }
        else 
        {
            StopCoroutine(TouchAttack());
            aiDestinationSetter.enabled = true;
            aiPath.enabled = true;
            hasTouchAttacked = false;
            return pursuitState;
        }

        return this; 
    }
    //Lets first make the enemy's speed zero when we start on the attack as to indicate to player that the enemy is doing the attack. Afterwards, we commit to the attack like usual

    private IEnumerator TouchAttack() 
    {
        
        Lunge();
        rb.AddForce(Vector2.zero,ForceMode2D.Impulse);
        yield return new WaitForSeconds(coolDownSeconds);
        hasTouchAttacked = true;
    }
    
    
    private void Lunge() 
    {
        Vector2 directionToPlayer = ((Vector2)ps.droplet.transform.position - rb.position).normalized;
        rb.AddForce(directionToPlayer * lungeStrength, ForceMode2D.Impulse);
        Debug.Log("I AM LUNGING!");
    }
    
}
