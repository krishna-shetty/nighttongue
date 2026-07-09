using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSpecificLevelTrigger : MonoBehaviour
{
    public void LoadSpecificLevelByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
}
