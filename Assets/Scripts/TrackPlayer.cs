using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPlayer : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Vector2 lastPosition;
    private Rigidbody2D rb;
    private Vector2 direction;
    public float speed;
    public float wait;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.Find("Player").transform;
        wait = 1f;
        StartCoroutine(FindLastPosition(wait));
    }
    public void FixedUpdate()
    {
        direction = (lastPosition - (Vector2) rb.position).normalized;
        rb.velocity = (direction * Time.deltaTime * speed);
    }
    public IEnumerator FindLastPosition(float duration)
    {
        while (true)
        {

            yield return new WaitForSeconds(duration);
            lastPosition = playerTransform.position;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastPosition, 0.2f);
       
    }

}
