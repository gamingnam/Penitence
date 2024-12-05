using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [HideInInspector] public float centerX,centerY,left,top,right,bottom;
    BoxCollider2D boxCollider;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        centerX = transform.position.x;
        centerY = transform.position.y;
        left = transform.position.x - boxCollider.bounds.extents.x;
        top = transform.position.y + boxCollider.bounds.extents.y;
        right = transform.position.x + boxCollider.bounds.extents.x;
        bottom = transform.position.y - boxCollider.bounds.extents.y;
    }
}



