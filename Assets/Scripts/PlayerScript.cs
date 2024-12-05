using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int ammo;
    private float health;

    [SerializeField] public InventoryManager inventory;

    [SerializeField] private float speed;
    private float hor;
    private float vert;
    private Vector2 dir;
    private Rigidbody2D rb;

    private Vector3 mouseWorldPosition;
    private float lookAngle;

    [SerializeField] private Transform firePoint;

    private Camera _cam;
    public float smooth = 0.5f;
    private Vector3 velocity = Vector3.zero;

    public AnimationCurve curve;
    public float duration = 1f;

    [SerializeField] private AudioClip gunShot;
    [SerializeField] private AudioClip gunNoAmmo;
    [SerializeField] private AudioClip gunEquip;
    [SerializeField] private AudioClip bulletCasing;

    [SerializeField] private Sprite normalJohn;
    [SerializeField] private Sprite hasGun;

    private SpriteRenderer sr;

    public Transform muzzle;
    public UnityEngine.Rendering.Universal.Light2D muzzleflash;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ammo = 7;
        _cam = Camera.main;
        muzzleflash = muzzle.GetComponent<UnityEngine.Rendering.Universal.Light2D>();

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
    }
    private void LateUpdate()
    {
        CameraHandler();
    }
    void FixedUpdate()
    {
        rb.velocity = dir * speed*Time.deltaTime;
    }

    private void ShootHandler()
    {
        if (Input.GetButtonDown("Fire1") && InventoryManager.isInventoryOpened == false)
        {
            if(ammo > 0) {
                StartCoroutine(Shake());
                muzzleflash.intensity = 50f;
                AudioSource.PlayClipAtPoint(gunShot, transform.position, 1f);
                RaycastHit2D hit = Physics2D.Raycast(firePoint.position, (Vector2)mouseWorldPosition -  (Vector2)firePoint.position);
                if (hit)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if(hit.collider.gameObject.tag == "Enemy")
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
        float xMidpoint = Mathf.Clamp((mouseWorldPosition.x - transform.position.x)/2,-magnitude,magnitude);
        float yMidpoint = Mathf.Clamp((mouseWorldPosition.y - transform.position.y) / 2, -magnitude,magnitude);

        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, new Vector3(transform.position.x+xMidpoint,transform.position.y+yMidpoint,-1f) , ref velocity, smooth);

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
            if(inventory.selectedItem != null)
                inventory.selectedItem.Use(this);
        }
    }
}
