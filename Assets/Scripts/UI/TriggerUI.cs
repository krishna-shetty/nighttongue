using TMPro;
using UnityEngine;

public class TriggerUI : MonoBehaviour
{
    [SerializeField] private GameObject UIObject;

    [Header("Optional Flavor Requirements")]

    [SerializeField, Tooltip("Leave empty if the UI is unaffected by what flavor the player has.")]
    private ActiveFlavorSO RequiredFlavor = null;

    [SerializeField, Tooltip("Object must contain a TextMeshProUGUI component.")]
    private GameObject TextObject;
    private TextMeshProUGUI _textMesh;

    [SerializeField, Tooltip("Text to display if the player has the required flavor.")]
    private string HasFlavorText;

    [SerializeField, Tooltip("Text to display if the player does not have the required flavor.")]
    private string NoFlavorText;

    private FlavorManager _flavorManager;
    private bool _isActive = false; // whether this object (not UI) is enabled

    private void Start()
    {
        if (RequiredFlavor)
        {
            _flavorManager = GameObject.FindGameObjectWithTag("Player").GetComponent<FlavorManager>();
            if (!_flavorManager) Debug.LogError("TriggerUI on " + gameObject + " :: Requires FlavorMananger.");

            _textMesh = TextObject ? TextObject.GetComponent<TextMeshProUGUI>() : null;
            //if (!_textMesh) Debug.LogError("TriggerUI on " + gameObject + " :: Requires a TextMeshProUGUI.");
        }

        ToggleUI(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || !other.CompareTag("PlayerModel")) return;
        ToggleUI(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isActive || !other.CompareTag("PlayerModel")) return;
        if (!UIObject) return;
        if (!UIObject.activeInHierarchy) ToggleUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerModel")) return;
        ToggleUI(false);
    }

    public void ToggleUI(bool active)
    {
        if (!UIObject) return;

        // change text if flavor dependent
        if (active && RequiredFlavor && _textMesh)
        {
            var activeFlavor = _flavorManager.GetActiveActiveFlavor();
            var passiveFlavor = _flavorManager.GetActivePassiveFlavor();

            if (RequiredFlavor == activeFlavor || RequiredFlavor == passiveFlavor) _textMesh.SetText(HasFlavorText);
            else _textMesh.SetText(NoFlavorText);
        }
        
        UIObject.SetActive(active);
    }

    private void OnEnable()
    {
        _isActive = true;
    }

    private void OnDisable()
    {
        _isActive = false;
        ToggleUI(false);
    }
}
