using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using TMPro;
using UnityEngine;

public class PlayerScript : MonoBehaviour,IDamageable
{
    #region General Variables
    [Header("General")]
    public int ammo;
    public float health;
    [SerializeField] private float speed;
    private Rigidbody2D rb;
	#endregion

	#region Movement Variables
	[Header("Movement")]
    private float hor;
    private float vert;
    private Vector2 dir;
    private Vector3 velocity = Vector3.zero;
	#endregion

	#region Camera Variables
	[Header("Camera")]
    private Camera _cam;
    private Vector3 mouseWorldPosition;
    private float lookAngle;
    public float smooth = 0.5f;
    public AnimationCurve curve;
    public float duration = 1f;
	#endregion

	#region Attacking Variables
	[Header("Attacking")]
	[SerializeField]private float enemyDamage;
	[SerializeField] private Transform firePoint;
    public Transform muzzle;
	#endregion


	#region Audio and SFX Variables
	[Header("Audio and SFX")]
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip gunNoAmmo;
    [SerializeField] private AudioClip bulletCasing;
    [SerializeField] private AudioSource audioSource;
    private ObjectPooler<AudioSource> gsPool;
    private ObjectPooler<AudioSource> gnsPool;
    private ObjectPooler<AudioSource> bcPool;

	#endregion

	#region Respawning Variables
	[Header("Respawning")]
    [SerializeField] private GameObject spawner;
    [SerializeField] private LayerMask spawnerMask;
    [SerializeField] private int spawnerRadius;
    public GameObject droplet;
	#endregion

	#region UI Variables
	[Header("UI")]
    public TextMeshProUGUI healthText; 
    [SerializeField] public InventoryManager inventory;
	#endregion

	#region Light2D Variables
	[Header("Light2D")]
	public UnityEngine.Rendering.Universal.Light2D muzzleflash;
	#endregion

	//[SerializeField] private Sprite normalJohn;
	//[SerializeField] private Sprite hasGun;

	//private SpriteRenderer sr;


	// Start is called before the first frame update
	void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        InstantiateDroplet(this.transform.position);
        muzzleflash = muzzle.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        healthText.text = "";
        health = 100f;
        gsPool = new ObjectPooler<AudioSource>(audioSource,ammo,null);
        gnsPool = new ObjectPooler<AudioSource>(audioSource,20,null);
        bcPool = new ObjectPooler<AudioSource>(audioSource,ammo,null);

    }

    #region Update Methods
    // Update is called once per frame
    void Update()
    {

        hor = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");

        dir = new Vector2(hor, vert).normalized;

        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lookAngle = Mathf.Atan2(mouseWorldPosition.y - transform.position.y, mouseWorldPosition.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(lookAngle - 90f, Vector3.forward);

        
        ShootHandler();
        InventoryHandler();
        RespawnParse();
        Respawn();
        InstantiateDroplet(this.transform.position);
        healthText.text = "Health: " + health;
    }
    private void LateUpdate()
    {
        CameraHandler();
    }
    void FixedUpdate()
    {
        rb.velocity = dir * speed * Time.deltaTime;
    }
    #endregion

    #region Shooting Methods
    private void ShootHandler()
    {
        if (Input.GetButtonDown("Fire1") && InventoryManager.isInventoryOpened == false)
        {
            if (ammo > 0)
            {
                StartCoroutine(Shake());
                muzzleflash.intensity = 50f;
                PlayGunShot();
                RaycastHit2D hit = Physics2D.Raycast(firePoint.position, (Vector2)mouseWorldPosition - (Vector2)firePoint.position);
                if (hit)
                {
                    if (hit.collider.gameObject.tag == "Enemy")
                    {
                        hit.collider.gameObject.GetComponent<Enemy>().ReceiveDamage(enemyDamage); //<-- enemyDamage variable can be changed later to be dynamic changeable based off enemy type (maybe with a scriptable object?)

					}
                }
                StartCoroutine(bulletShellSound());
                ammo--;
            }
            else
            {
                PlayNoAmmo();
            }
        }
        muzzleflash.intensity -= 2f;
        muzzleflash.intensity = Mathf.Clamp(muzzleflash.intensity, 0f, 50f);
    }

    private void PlayGunShot()
    {
        AudioSource audioSource = gsPool.Get(transform.position,Quaternion.identity);
        audioSource.clip = gunShot; // Ensure the correct sound is assigned
        audioSource.Play();
        
        StartCoroutine(ReturnToGunShotPool(audioSource, audioSource.clip.length)); // Return after sound finishes
    }

    private void PlayNoAmmo()
    {
		AudioSource audioSource = gnsPool.Get(transform.position,Quaternion.identity);
        audioSource.clip = gunNoAmmo; // Ensure the correct sound is assigned
        audioSource.Play();
        
        StartCoroutine(ReturnToGunNoAmmoPool(audioSource, audioSource.clip.length)); // Return after sound finishes
    }

    public IEnumerator bulletShellSound()
    {
        yield return new WaitForSeconds(0.25f);
        AudioSource audioSource  = bcPool.Get(transform.position,Quaternion.identity);
        audioSource.clip = bulletCasing;
        audioSource.Play();
        StartCoroutine(ReturnToBulletCasePool(audioSource, audioSource.clip.length));

    }
    IEnumerator ReturnToGunShotPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        gsPool.ReturnToPool(source);
    }

    IEnumerator ReturnToGunNoAmmoPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        gnsPool.ReturnToPool(source);
    }
    IEnumerator ReturnToBulletCasePool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        bcPool.ReturnToPool(source);
    }
	#endregion

	#region Camera Methods
	private void CameraHandler()
    {
        float magnitude = 2f;
        float xMidpoint = Mathf.Clamp((mouseWorldPosition.x - transform.position.x) / 2, -magnitude, magnitude);
        float yMidpoint = Mathf.Clamp((mouseWorldPosition.y - transform.position.y) / 2, -magnitude, magnitude);

        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, new Vector3(transform.position.x + xMidpoint, transform.position.y + yMidpoint, -1f), ref velocity, smooth);

    }
    IEnumerator Shake()
    {
        Vector2 startPosition = (Vector2)_cam.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float mag = curve.Evaluate(elapsedTime / duration);
            Vector2 offset = Random.insideUnitCircle * mag;
            _cam.transform.position = new Vector3(startPosition.x + offset.x, startPosition.y + offset.y, _cam.transform.position.z);
            yield return null;
        }

        _cam.transform.position = new Vector3(startPosition.x, startPosition.y, _cam.transform.position.z);
    }
	#endregion

	#region Inventory Methods
	private void InventoryHandler()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventory.selectedItem != null)
                inventory.selectedItem.Use(this);
        }
    }
	#endregion

	#region Respawn Methods
	void RespawnParse()
    {
        Collider2D[] circleCols = Physics2D.OverlapCircleAll(this.transform.position, spawnerRadius, spawnerMask);
		for (int i = 0; i < circleCols.Length; i++)
		{
            Collider2D circleCol = circleCols[i];
			if (circleCol == spawner || circleCol == null)
			{
                continue; 
			}

            spawner = circleCol.gameObject;
            break;
		}
    }
    //Down the line change this an IEnumator where it waits for the Taste/Death Animation to finish before Respawning
    void Respawn()
    {
        if(health <= 0)
        {
            this.transform.position = spawner.transform.position;
            health = 100;
        }
    }
	#endregion

	#region Health Methods
	public void UpdateHealth(float newHealthValue)
   {
        health = newHealthValue;
   }
   public void ReceiveDamage(float damage)
   {
        var updatedHealth = health - damage;
        UpdateHealth(updatedHealth > 0 ? updatedHealth : 0);
   }
	#endregion;

	#region Player Tracking Methods
	private GameObject InstantiateDroplet(Vector2 position)
    {
        if (droplet != null)
        {
            Destroy(droplet);
        }
        droplet = new GameObject("Droplet");
        droplet.transform.position = position;
        CircleCollider2D cirCollider = droplet.AddComponent<CircleCollider2D>(); // Add a collider to the point
        cirCollider.isTrigger = true; // Set collider as trigger
        droplet.tag = "Droplet";
        return droplet;
    }
	#endregion

	#region Collision Methods
	private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) 
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			ReceiveDamage(enemy.EnemyDmg);

			//This code below is technically useless as the enemies rigidbodies are static, and cannot apply a force
			//Maybe we find someway for the player to do it instead?

			/*Transform enemyTransform = collision.gameObject.GetComponent<Transform>();
            Vector2 direction = (rb.position - (Vector2)enemyTransform.position).normalized;
            Debug.Log(direction);
            ApplyKnockBack(direction, 9000f);*/

		}
	}
	private void ApplyKnockBack(Vector2 direction, float strength)
    {
        Debug.Log("Applying Knockback");
        this.rb.AddForce(direction * strength,ForceMode2D.Impulse);
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, spawnerRadius);
    }
}