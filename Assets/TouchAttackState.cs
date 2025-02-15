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
    [SerializeField] private float touchAttackSpeed;
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


    //Second
  
        void Start()
        {
            aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
            aiPath = enemy.GetComponent<AIPath>();
            player = GameObject.FindGameObjectWithTag("Player");
            ps = player.GetComponent<PlayerScript>();
        }

        public override State RunCurrentState()
        {
            if (!hasTouchAttacked)
            {
                StartCoroutine(TouchAttack());
                //TouchAttack();
            }
            else
            {
                aiDestinationSetter.enabled = true;
                aiPath.enabled = true;
                hasTouchAttacked = false;
                return pursuitState;
            }

            return this;
        }

        private IEnumerator TouchAttack()
        {
            Lunge(); // Perform the lunge attack

            yield return new WaitForSeconds(0.5f); // Short pause to let the lunge happen

            yield return new WaitForSeconds(coolDownSeconds); // Additional cooldown before resuming pursuit
            hasTouchAttacked = true;
        }

        private void Lunge()
        {
            // Stop AI movement completely
            aiDestinationSetter.enabled = false;
            aiPath.enabled = false;
            rb.velocity = Vector2.zero; // Ensure a clean leap forward

            // Calculate direction to the last known droplet position
            Vector2 directionToPlayer = ((Vector2)ps.droplet.transform.position - rb.position).normalized;

            // Apply a strong impulse force for a single leap
            rb.AddForce(directionToPlayer * lungeStrength, ForceMode2D.Impulse);

            Debug.Log("LUNGE ATTACK INITIATED!");
        }


    //First
    /*
    void Start()
    {
        aiDestinationSetter = enemy.GetComponent<AIDestinationSetter>();
        aiPath = enemy.GetComponent<AIPath>();
        aiLerp = enemy.GetComponent<AILerp>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        ps = player.GetComponent<PlayerScript>();
        //rb.AddForce(Vector2.zero, ForceMode2D.Impulse);
    }
    */
    /*
    public override State RunCurrentState()
    {
        aiLerp.speed = touchAttackSpeed;
        if (!hasTouchAttacked)
        {
            //pursuitState.enabled = false;
            aiDestinationSetter.enabled = false;
            aiPath.enabled = false;
            StartCoroutine(TouchAttack());
            //TouchAttack();
        }
        else 
        {
            aiDestinationSetter.enabled = true;
            aiPath.enabled = true;
            hasTouchAttacked = false;
            return pursuitState;
            /*
            //TouchAttack();
            StopCoroutine(TouchAttack());
            aiDestinationSetter.enabled = true;
            aiPath.enabled = true;
            hasTouchAttacked = false;
            //pursuitState.enabled = true;
            return pursuitState;
            */
    //}
    //return this; 
    //}
    /*
    private IEnumerator TouchAttack() 
    {
        
        Lunge();
        //rb.AddForce(Vector2.zero,ForceMode2D.Impulse);
        yield return new WaitForSeconds(coolDownSeconds);
        hasTouchAttacked = true;
    }
    */

    /*
    private void Lunge() 
    {
        Vector2 directionToPlayer = ((Vector2)ps.droplet.transform.position - rb.position).normalized;
        //rb.AddForce(directionToPlayer * lungeStrength, ForceMode2D.Impulse);
        rb.velocity = directionToPlayer * aiLerp.speed;
        Debug.Log("I AM EDGING!");
    }
    */


}
