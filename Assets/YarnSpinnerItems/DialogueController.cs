using UnityEngine;
using Yarn.Unity;

public class DialogueController : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public string yarnNode = "Working";
    public string yarnNodeAv1 = "TamiICanTaste";
    public string yarnNodeAv2 = "Alveolus2";

    private bool playerInRange = false;

    //public DialogueRunner dialogueRunnerAv1;

    public void GoToNode()
    {
        Debug.Log("almost...");
        dialogueRunner.StartDialogue(yarnNode);
        Debug.Log("it went!!");
    }

    public void GoToTamiSweetSmokingAv()
    {
        Debug.Log("tami...");
        dialogueRunner.Stop();
        dialogueRunner.StartDialogue(yarnNodeAv1);
        Debug.Log("speak!");
    }

    public void GoTalk2Av2()
    {
        Debug.Log("tami...");
        dialogueRunner.StartDialogue(yarnNodeAv2);
        Debug.Log("speakie!");
    }

    ///


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            dialogueRunner.StartDialogue(yarnNode);
        }
    }
}
