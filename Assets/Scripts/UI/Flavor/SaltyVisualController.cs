using UnityEngine;

public class SaltyVisualController : MonoBehaviour
{
    [SerializeField] private FlavorManager _flavorManager;
    [SerializeField] private GameObject _transformUI;

    private void Awake()
    {
        if (!_flavorManager)
            _flavorManager = GetComponent<FlavorManager>();
    }

    private void OnEnable()
    {
        _flavorManager.OnPickupFlavor += HandleFlavorChanged;
    }

    private void OnDisable()
    {
        _flavorManager.OnPickupFlavor -= HandleFlavorChanged;
    }

    private void HandleFlavorChanged(FlavorSO flavor)
    {
        if ( _transformUI)
        {
            _transformUI.SetActive(flavor is SaltyFlavorSO);
        }  
    }
}