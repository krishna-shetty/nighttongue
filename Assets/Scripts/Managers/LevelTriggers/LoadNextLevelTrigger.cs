using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextLevelTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "PlayerModel";

    [Tooltip("Leave empty to default to next level in the build order")]
    public string LevelToLoad = null;

    [Tooltip("For enabling the heart level when returning to bartender")]
    public int MaxLevelToSet = -1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Entered load trigger");
            if (LevelToLoad == "" || LevelToLoad == null)
            {
                LoadingHelper.instance.GetAnimator().SetTrigger("Next");
                StartCoroutine(Co_NextLevel());
            }
            else
            {
                if (MaxLevelToSet > -1) SaveManager.Instance.SaveMaxLevel(MaxLevelToSet);
                LoadingHelper.instance.GetAnimator().SetTrigger("Next");
                StartCoroutine(Co_LoadLevel());
            }
        }
    }

    IEnumerator Co_NextLevel()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        LevelManager.Instance.LoadNextLevel();
    }

    IEnumerator Co_LoadLevel()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        LevelManager.Instance.LoadScene(LevelToLoad);
    }
}
