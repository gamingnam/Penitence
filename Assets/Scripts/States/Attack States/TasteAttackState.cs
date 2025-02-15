using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TasteAttackState : State
{
    [SerializeField] private State idleState;
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private bool hasTasted;
    [SerializeField] private Animator tasteAttackAnimator;
    [SerializeField] private Animation tasteAttackAnimation;

    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }
    public override State RunCurrentState()
    {
        if(!hasTasted)
        {
            playerScript.ReceiveDamage(playerScript.health);
            hasTasted = true;
            return idleState;
        }
        if(hasTasted)
        {
            hasTasted = false;
        }
        
        return this;
    }
}
