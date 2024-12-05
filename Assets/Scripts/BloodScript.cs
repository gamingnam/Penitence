using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodScript : MonoBehaviour
{
    private Vector3 localscale;
    public float despawnTime;
    private Color newColor;
    private Color oldColor;
    private float alpha;
    private float size;
    

    // Start is called before the first frame update
    void Start()
    {
        newColor = GetComponent<SpriteRenderer>().color;
        oldColor = GetComponent<SpriteRenderer>().color;
        alpha = newColor.a;
        size = Random.Range(0.5f, 1.5f);
        this.transform.localScale = new Vector3(size, size, 0f);
        localscale = this.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        despawnTime -= Time.deltaTime;
        if (despawnTime < 0)
        {
            newColor.a -= 0.001f;
            GetComponent<SpriteRenderer>().color = Color.Lerp(oldColor, newColor, 1f);

            //local scale =;
            if (newColor.a <= 0) Destroy(gameObject);
        }
    }
}
