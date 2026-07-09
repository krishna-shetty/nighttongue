using UnityEngine;
using System.Collections.Generic;

public class SwingPointHighlightManager : MonoBehaviour
{
    private SwingTargetResolver _resolver;
    private GameObject _currentHighlight;
    public GameObject _highlightUI_prefab;
    private GameObject _highlightUI_current;

    [SerializeField] private Material _highlightMaterial;
    private Material _originalMaterial;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _resolver = player.GetComponent<SwingTargetResolver>();

        if (_resolver == null)
        {
            Debug.LogError("SwingPointHighlightManager: No SwingTargetResolver found on the player.");
            return;
        }
    }

    void Update()
    {
        if (_highlightMaterial == null) return;

        Collider closest = _resolver.CurrentTarget; 

        GameObject closestGO = closest?.gameObject;

        if (closestGO == null)
        {
            ClearHighlight();
            return;
        }

        if (closestGO == _currentHighlight) return;

        ClearHighlight();

        var r = closestGO.GetComponent<Renderer>();
        if (r != null)
        {
            _originalMaterial = r.material;
            r.material = _highlightMaterial;
            _currentHighlight = closestGO;
            if (_highlightUI_prefab != null)
            {
                Vector3 pos = closestGO.transform.position;
                _highlightUI_current = Instantiate(_highlightUI_prefab, closestGO.transform.position, Quaternion.identity);
                _highlightUI_current.transform.SetParent(closestGO.transform);
            }
        }
    }

    private void ClearHighlight()
    {
        if (_currentHighlight != null)
        {
            var r = _currentHighlight.GetComponent<Renderer>();
            if (r != null && _originalMaterial != null)
                r.material = _originalMaterial;
        }

        _currentHighlight = null;
        _originalMaterial = null;
        
        if (_highlightUI_current != null)
        {
            Destroy(_highlightUI_current);
            _highlightUI_current = null;
        }
    }
}