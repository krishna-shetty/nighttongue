using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("subscribing to loading");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        transform.parent.gameObject.SetActive(false);
    }
}
