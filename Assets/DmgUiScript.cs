using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DmgUiScript : MonoBehaviour
{
    public float health;
    public float alpha;
    public Image outerBlood;
    public Image fillBlood;
    // Start is called before the first frame update
    void Start()
    {
        health = GameObject.Find("Player").GetComponent<PlayerScript>().health;
        outerBlood = this.transform.GetChild(0).GetComponent<Image>();
        fillBlood = this.transform.GetChild(1).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        alpha = 1 - (health / 100f);
        outerBlood.color = new Color(outerBlood.color.r, outerBlood.color.g, outerBlood.color.b, alpha);
        fillBlood.color = new Color(outerBlood.color.r, outerBlood.color.g, outerBlood.color.b, alpha-0.25f);
    }

}
