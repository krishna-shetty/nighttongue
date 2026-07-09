using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponent<TextMeshProUGUI>().color = new Color32(241, 159, 157, 255);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<TextMeshProUGUI>().color = new Color32(158, 86, 89, 255);
    }
}
