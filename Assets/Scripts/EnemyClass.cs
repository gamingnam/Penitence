using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Enemy : MonoBehaviour,IDamageable
{
    public int MaxHp;

    public int EnemyDmg;

    public float _currentHealth;

    public float knockback; 

    [SerializeField] private ParticleSystem bloodSpray;
    [SerializeField] private GameObject bloodDrop;

    [SerializeField] private AudioClip hurtSound;
    private AudioSource _audioSource;

    public void Start()
    {
        _currentHealth = MaxHp;
        _audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if(_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void UpdateHealth(float newHealthValue)
    {
        _currentHealth = newHealthValue;
    }

    public void ReceiveDamage(float damage)
    {
        var updatedHealth = _currentHealth - damage;
        UpdateHealth(updatedHealth > 0 ? updatedHealth : 0);
        Instantiate(bloodSpray, transform.position, Quaternion.identity);
        //DropBlood(5, 1f);
        AudioSource.PlayClipAtPoint(hurtSound, transform.position, 1f);

    }
   /* public void DropBlood(int amount, float spread)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(bloodDrop, (Vector2)(transform.position + Random.insideUnitSphere * spread), Quaternion.identity);
        }
    }*/
    public void RotateToPlayer(GameObject player)
    {
        Vector3 direction = player.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
