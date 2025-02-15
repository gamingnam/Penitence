using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScript : MonoBehaviour
{
    private RectTransform rectTransform;
    private TextMeshProUGUI textDisplay;
    private Image img;
    public string fullText;
    private string curText = "";
    public float delay = 0.01f;
    public AudioClip textSound;
    private bool isTextPlaying;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        img = GetComponentInChildren<Image>();
        isTextPlaying = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isTextPlaying)
            {
                StartCoroutine(DisplayText());
                isTextPlaying = true;
            }
            else
            {
                StopCoroutine(DisplayText());
                isTextPlaying = false;
            }
        }
    }
    IEnumerator DisplayText()
    {
        for (int i = 0; i < fullText.Length + 1; i++)
        {
            curText = fullText.Substring(0, i);
            textDisplay.text = curText;
            yield return new WaitForSeconds(delay);
        }
    }
    IEnumerator MoveFromTo(Vector3 from, Vector3 to, float speed, RectTransform transform)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += speed * Time.deltaTime;
            transform.anchoredPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }
}
