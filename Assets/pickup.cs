using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] private GameObject inventory;
    private InventoryManager inventoryManager;
    [SerializeField] private WeapClass weapon;
    [SerializeField] private AudioClip pickupSound;
    void Start()
    {
        inventoryManager = inventory.GetComponent<InventoryManager>();
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            inventoryManager.Add(weapon,1);
            AudioSource.PlayClipAtPoint(pickupSound, transform.position,1f);
            Destroy(gameObject);
        }
    }
}
