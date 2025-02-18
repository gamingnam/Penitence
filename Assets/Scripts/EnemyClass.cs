using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Enemy : MonoBehaviour,IDamageable
{
    //
    public int MaxHp;

    public int EnemyDmg;

    public float _currentHealth;

    public float knockback; 

    [SerializeField] private GameObject bloodSpray;
    [SerializeField] private GameObject bloodDrop;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip hurtSound;
    private ObjectPooler<GameObject> bloodDropPool;
    private ObjectPooler<GameObject> hurtSoundPool;
    private ObjectPooler<GameObject> bloodSprayPool;
    

    public void Start()
    {
        _currentHealth = MaxHp;
        bloodDropPool = new ObjectPooler<GameObject>(bloodDrop,5,null);
        hurtSoundPool = new ObjectPooler<GameObject>(_audioSource.gameObject,10,null);
        bloodSprayPool = new ObjectPooler<GameObject>(bloodSpray,15,null);

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
        DropBlood(5, 1f);
        PlayHurtSound();

    }
   public void DropBlood(int amount, float spread)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject blood = bloodDropPool.Get((Vector2)(transform.position + Random.insideUnitSphere * spread), Quaternion.identity);
            StartCoroutine(ReturnBloodToPool(blood,5.1f));
        }
    }

    private void SprayBlood()
    {
        GameObject bloodSprayObj = bloodSprayPool.Get(transform.position,Quaternion.identity);
        ParticleSystem particleSystem = bloodSprayObj.GetComponent<ParticleSystem>();
        particleSystem.Play();
        StartCoroutine(ReturnBloodSprayToPool(bloodSprayObj,2.1f));
    }

    private void PlayHurtSound()
    {
        GameObject soundObj = hurtSoundPool.Get(transform.position, Quaternion.identity); // Get GameObject
        AudioSource audioSource = soundObj.GetComponent<AudioSource>(); // Get AudioSource component
        audioSource.clip = hurtSound; // Ensure the correct sound is assigned
        audioSource.Play();
        
        StartCoroutine(ReturnSoundToPool(soundObj, audioSource.clip.length)); // Return after sound finishes
    }

    private IEnumerator ReturnBloodToPool(GameObject blood, float delay)
    {
        yield return new WaitForSeconds(delay);
        bloodDropPool.ReturnToPool(blood);
    }

    private IEnumerator ReturnBloodSprayToPool(GameObject bloodSpray, float delay)
    {
        yield return new WaitForSeconds(delay);
        bloodDropPool.ReturnToPool(bloodSpray);
    }

   private IEnumerator ReturnSoundToPool(GameObject soundObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        soundObj.SetActive(false);
        hurtSoundPool.ReturnToPool(soundObj);
    }
} 
