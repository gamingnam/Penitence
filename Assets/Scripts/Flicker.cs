using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flicker : MonoBehaviour
{
    private UnityEngine.Rendering.Universal.Light2D flame;
    public float maxIntensity;
    public float minIntensity;
    public float minFlicker;
    public float maxFlicker;
    public bool canFlicker;
    // Start is called before the first frame update
    void Start()
    {
        flame = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canFlicker)
        {
            flame.intensity += Random.Range(minFlicker, maxFlicker);
            flame.intensity = Mathf.Clamp(flame.intensity, minIntensity, maxIntensity);
        }
        else
        {
            flame.intensity = 0f;
        }
        

    }
}
