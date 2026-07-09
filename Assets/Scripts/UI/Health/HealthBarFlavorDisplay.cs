using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarFlavorDisplay : MonoBehaviour
{
    private FlavorManager _flavorManager;
    [SerializeField] private Image _health1;
    [SerializeField] private Image _health2;
    [SerializeField] private Image _health3;

    [SerializeField] private Sprite _defaultIcon;
    [SerializeField] private Sprite _sweetIcon;
    [SerializeField] private Sprite _saltyIcon;

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("HealthBarFlavorDisplay could not find a GameObject with the 'Player' tag.");
            return;
        }

        _flavorManager = player.GetComponent<FlavorManager>();
        if (_flavorManager == null)
        {
            Debug.LogError("HealthBarFlavorDisplay could not find a FlavorManager on the Player.");
            return;
        }

        _flavorManager.OnPickupFlavor += HandleFlavorPickup;
    }

    private void OnDestroy()
    {
        if (_flavorManager != null)
            _flavorManager.OnPickupFlavor -= HandleFlavorPickup;
    }

    private void HandleFlavorPickup(FlavorSO flavor)
    {
        UpdateHealthIcons(flavor);
    }

    private void UpdateHealthIcons(FlavorSO flavor)
    {
        Sprite icon = GetIconForFlavor(flavor.Flavor);
        _health1.sprite = icon;
        _health2.sprite = icon;
        _health3.sprite = icon;
        _health1.SetNativeSize();
        _health2.SetNativeSize();
        _health3.SetNativeSize();
    }

    private Sprite GetIconForFlavor(EFlavor flavorType)
    {
        switch (flavorType)
        {
            case EFlavor.Sweet:
                return _sweetIcon;
            case EFlavor.Salty:
                return _saltyIcon;
            default:
                return _defaultIcon;
        }
    }
}