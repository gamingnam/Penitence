using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemClass : ScriptableObject
{
    [Header("Item")]
    public string itemName;
    public Sprite itemIcon;
    public string description;
    public bool isStackable = true;
    public bool isDroppable;
    public GameObject itemObject;
    public float equipSpeed;

    public virtual void Use(PlayerScript caller) // rename method
    {
        Debug.Log("used item");
    }
    public virtual ItemClass GetItem() { return this; }
    public virtual WeapClass GetWeap() { return null; }
    public virtual MiscClass GetMisc() { return null; }
    public virtual ConsumableClass GetConsumable() {  return null; }

}
