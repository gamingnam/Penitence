using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


[CreateAssetMenu(fileName = "new Weapon Class", menuName = "Item/Weapon")]
public class WeapClass : ItemClass
{
    [Header("Weapon")]
    public WeaponType weaponType;
    public int weaponDamage;
    public int ammoCapacity;
    public float _fireRate;
    public float firingError;
    public float damageFalloffRange;
    public float damageFalloff;
    public float reloadSpeed;
    public AudioClip attackSound;
    public AudioClip reloadSound;
    public enum WeaponType { gun, melee }

    public override void Use(PlayerScript caller)
    {
        base.Use(caller);
        Debug.Log("Attack");
    }
    public virtual void Shoot(PlayerScript caller)
    {
    }
    public override WeapClass GetWeap() { return this; }

}
