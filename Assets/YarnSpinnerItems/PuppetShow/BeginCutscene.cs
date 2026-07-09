using UnityEngine;

public class BeginCutscene : MonoBehaviour
{
    public GameObject StartTalking;
    public DialogueController dialogueController;


    void Start()
    {
        Debug.Log("heyy mamo!");
        dialogueController.GoToNode();
        //Debug.Log("omw!!")
    }
}



