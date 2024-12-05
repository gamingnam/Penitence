using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class jumpscare : MonoBehaviour
{
    [SerializeField] GameObject img;
    [SerializeField] AudioClip sfx;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            img.SetActive(true);
            AudioSource.PlayClipAtPoint(sfx, transform.position, 1f);
            Destroy(gameObject);
        }
    }
}
