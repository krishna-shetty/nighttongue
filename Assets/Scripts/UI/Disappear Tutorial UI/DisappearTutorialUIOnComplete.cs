using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisappearTutorialUIOnComplete : MonoBehaviour
{
    public InputActionReference InputAction;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerModel"))
        {
            EnableObjects();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (InputAction.action.IsPressed()) DisableObject();
    }

    private void EnableObjects()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void DisableObject(InputAction.CallbackContext ctx = new())
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
