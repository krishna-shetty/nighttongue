using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingDots : MonoBehaviour
{

    [SerializeField]
    private Transform[] loadingDots;
    public float animTime = 1f;
    public float bounceTime = 0.25f;
    public float bounceDistance = 10f;
    private float initialYPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("loading called");
        initialYPos = loadingDots[0].localPosition.y;
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

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < loadingDots.Length; i++)
        {
            Vector3 p = loadingDots[i].localPosition;
            float t = Time.time * animTime * Mathf.PI + p.x;
            float y = (Mathf.Cos(t) - bounceTime) / (1f - bounceTime);
            p.y = Mathf.Max(0, y * bounceDistance) + initialYPos;
            loadingDots[i].localPosition = p;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("deactive loading////////////////////////////////////////////////////////////////////");
        transform.parent.gameObject.SetActive(false);
    }
}
