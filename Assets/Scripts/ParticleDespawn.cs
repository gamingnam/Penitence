using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDespawn : MonoBehaviour
{
    [SerializeField] private float timeUntilDespawn;
    private float timer = 0f;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeUntilDespawn)
            Destroy(gameObject);
    }
}
