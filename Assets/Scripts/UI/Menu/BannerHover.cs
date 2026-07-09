using UnityEngine;
using UnityEngine.EventSystems;

public class BannerHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private float bannerRotation;
    private Vector3 currRotation;
    [SerializeField]
    private GameObject banner;

    private void Awake()
    {
        currRotation = transform.eulerAngles;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RotateBanner();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RevertBanner();
    }

    private void RotateBanner()
    {
        Vector3 newRotation = new Vector3(currRotation.x, currRotation.y, bannerRotation);
        banner.transform.rotation = Quaternion.Euler(newRotation);
    }

    private void RevertBanner()
    {
        banner.transform.rotation = Quaternion.Euler(currRotation);
    }
}
