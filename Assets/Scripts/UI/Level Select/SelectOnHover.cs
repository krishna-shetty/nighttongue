using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent PointerEnter;
    public UnityEvent PointerExit;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
        CanvasController.Active?.SetLastSelected(gameObject);
        PointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        var device = InputManager.Instance.LastUsedDevice;
        if (device is Keyboard ||
            device is Mouse)
        {
            Debug.Log("Deselecting " + gameObject.name);
            EventSystem.current.SetSelectedGameObject(null);
            PointerExit?.Invoke();
        }
    }

}