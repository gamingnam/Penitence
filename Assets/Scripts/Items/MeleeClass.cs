using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


[CreateAssetMenu(fileName = "new Melee Class", menuName = "Item/Melee")]
public class MeleeClass : ItemClass
{
    [Header("Melee")]
    public WeaponType weaponType;
    public int weaponDamage;
    public float atkRate;
    public float atkRange;
    public AudioClip attackSound;
    public enum WeaponType { bat, crowbar }

    public override void Use(PlayerScript caller)
    {
        base.Use(caller);
        Debug.Log("Attack");
    }
    public override MeleeClass GetMelee() { return this; }

}
