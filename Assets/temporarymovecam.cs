using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temporarymovecam : MonoBehaviour
{
    [SerializeField] float speed;
    Vector2 direction;
    private float hor;
    private float vert;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hor = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");
        direction = new Vector2(hor, vert);
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
