using UnityEngine;

public class TamiFlavorDialogue : MonoBehaviour
{
    public GameObject SweetPickup_1;
    public DialogueController dialogueController;


    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("PlayerModel"))
        {
            Debug.Log("its sweet time!");
            dialogueController?.GoToTamiSweetSmokingAv();
            //Debug.Log("omw!!");

        }


    }
}
