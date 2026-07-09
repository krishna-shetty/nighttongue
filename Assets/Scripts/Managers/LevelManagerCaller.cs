using System.Collections;
using UnityEngine;

public class LevelManagerCaller : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (LevelManager.Instance != null)
        {
            LoadingHelper.instance.GetAnimator().SetTrigger("Next");
            StartCoroutine(Co_LoadLevel(sceneName));
        }
        else
            Debug.LogError("LevelManager.Instance is null!");
    }

    IEnumerator Co_LoadLevel(string sceneName)
    {
        yield return new WaitForSecondsRealtime(1.5f);
        LevelManager.Instance.LoadScene(sceneName);
    }
}