using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [SerializeField] private string _conditionTag = "Boat";
    private GameObject _platform;
    private ConditionalMovingPlatform _movingPlatform;
    private Collider _platformCollider;

    private void Awake()
    {
        var parent = transform.parent;
        if (parent == null)
        {
            Debug.LogError("TriggerDetector :: No parent platform found.");
            enabled = false;
            return;
        }

        _platform = parent.gameObject;

        _movingPlatform = _platform.GetComponent<ConditionalMovingPlatform>();
        if (_movingPlatform == null)
        {
            Debug.LogError("TriggerDetector :: ConditionalMovingPlatform not found on parent.");
            enabled = false;
            return;
        }

        _platformCollider = _platform.GetComponent<Collider>();
        if (_platformCollider == null)
        {
            Debug.LogError("TriggerDetector :: No Collider found on parent platform.");
            enabled = false;
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(_conditionTag)) return;

        (float, float) boatSize = MeshSize.GetSize(other.gameObject);
        (float, float) platformSize = MeshSize.GetSize(_platform);

        bool boatHorizontallyInside =
            (other.transform.position.x - boatSize.Item1 / 2) >= (_platform.transform.position.x - platformSize.Item1 / 2) &&
            (other.transform.position.x + boatSize.Item1 / 2) <= (_platform.transform.position.x + platformSize.Item1 / 2);

        float boatBottom = other.transform.position.y - boatSize.Item2 / 2;
        float platformTop = _platform.transform.position.y + platformSize.Item2 / 2;

        bool boatIsOnTop =
            boatBottom >= platformTop;

        if (boatHorizontallyInside && boatIsOnTop)
        {
            _movingPlatform.CanMove = true;

            ConditionalMovingPlatform otherPlatform = other.GetComponentInParent<ConditionalMovingPlatform>();
            
            if (otherPlatform != null)
            {
                otherPlatform.CanMove = false;
            }

            other.transform.SetParent(_platform.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_conditionTag)) return;
        _movingPlatform.CanMove = false;
        other.transform.SetParent(null);
    }
}
