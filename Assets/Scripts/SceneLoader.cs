using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider slider;
    public void LoadScene (int sceneId)
    {
        StartCoroutine(LoadAsync(sceneId));
    }
    public IEnumerator LoadAsync(int sceneId) {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        loadingScreen.SetActive(true);
        while (!operation.isDone) { 
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(progress);
            slider.value = progress;
            yield return null;
        }
    }
}
