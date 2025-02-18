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
    #region General
    [Header("General")]
    public int ammo;
    public float health;
    [SerializeField] private float speed;
    private Rigidbody2D rb;
    #endregion
   
    #region Movement 
    [Header("Movement")]
    private float hor;
    private float vert;
    private Vector2 dir;
    private Vector3 velocity = Vector3.zero;
    #endregion
   
    #region Camera
    [Header("Camera")]
    private Camera _cam;
    private Vector3 mouseWorldPosition;
    private float lookAngle;
    public float smooth = 0.5f;
    public AnimationCurve curve;
    public float duration = 1f;
    #endregion

    #region Attacking
    [Header("Attacking")]
    [SerializeField] private Transform firePoint;
    public Transform muzzle;
    #endregion


    #region Audio and SFX
    [Header("Audio and SFX")]
    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip gunNoAmmo;
    [SerializeField] private AudioClip bulletCasing;
    #endregion

    #region Respawning
    [Header("Respawning")]
    [SerializeField] private GameObject spawner;
    [SerializeField] private LayerMask spawnerMask;
    [SerializeField] private int spawnerRadius;
    public GameObject droplet;
    #endregion

    #region UI
    [Header("UI")]
    public TextMeshProUGUI healthText; 
    [SerializeField] public InventoryManager inventory;
    #endregion

    #region Light2D
    public UnityEngine.Rendering.Universal.Light2D muzzleflash;
    #endregion

     //[SerializeField] private Sprite normalJohn;
    //[SerializeField] private Sprite hasGun;

    //private SpriteRenderer sr;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //ammo = 7;
        _cam = Camera.main;
        InstantiateDroplet(this.transform.position);
        muzzleflash = muzzle.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        healthText.text = "";
        health = 100f;

    }

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

    private void ShootHandler()
    {
        if (Input.GetButtonDown("Fire1") && InventoryManager.isInventoryOpened == false)
        {
            if (ammo > 0)
            {
                StartCoroutine(Shake());
                muzzleflash.intensity = 50f;
                AudioSource.PlayClipAtPoint(gunShot, transform.position, 1f);
                RaycastHit2D hit = Physics2D.Raycast(firePoint.position, (Vector2)mouseWorldPosition - (Vector2)firePoint.position);
                if (hit)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.gameObject.tag == "Enemy")
                    {
                        hit.collider.gameObject.GetComponent<Enemy>().ReceiveDamage(30);
                    }
                }
                StartCoroutine(bulletShellSound());
                ammo--;
            }
            else
            {
                AudioSource.PlayClipAtPoint(gunNoAmmo, transform.position, 1f);
            }
        }
        muzzleflash.intensity -= 2f;
        muzzleflash.intensity = Mathf.Clamp(muzzleflash.intensity, 0f, 50f);
    }

    public IEnumerator bulletShellSound()
    {
        yield return new WaitForSeconds(0.25f);
        AudioSource.PlayClipAtPoint(bulletCasing, transform.position, 1f);
    }

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

    private void InventoryHandler()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventory.selectedItem != null)
                inventory.selectedItem.Use(this);
        }
    }

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

   public void UpdateHealth(float newHealthValue)
   {
        health = newHealthValue;
   }
   public void ReceiveDamage(float damage)
   {
        var updatedHealth = health - damage;
        UpdateHealth(updatedHealth > 0 ? updatedHealth : 0);
   }

   void ApplyKnockBack(Vector2 direction, float force)
    {
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, spawnerRadius);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) 
        {
            ReceiveDamage(5f);
            Transform enemyTransform = collision.gameObject.GetComponent<Transform>();
            Vector2 directionToPlayer = ((Vector2)enemyTransform.transform.position - this.rb.position).normalized;
            Debug.Log(directionToPlayer);
            rb.AddForce(-directionToPlayer * 30f,ForceMode2D.Impulse);
            
        }
    }
}
