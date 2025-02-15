using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class PupilFollow : MonoBehaviour    
{   
    public Vector2 mousePos;
    public Vector2 move;
    private float speed = 0.01f;
    public Rigidbody2D rb;
    [SerializeField] float xMax;
    [SerializeField] float xMin;
    [SerializeField] float yMax;
    [SerializeField] float yMin;
    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }
    void Update()   
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = Vector2.MoveTowards(transform.position,mousePos,speed);
        transform.position = new Vector2(Mathf.Clamp(transform.position.x, xMin, xMax), Mathf.Clamp(transform.position.y, yMin, yMax));
        // transform.position = new Vector2(Mathf.Clamp(transform.position.x, -1.2f, -0.9f), Mathf.Clamp(transform.position.y, 0.88f, 1.08f));
    }
}
