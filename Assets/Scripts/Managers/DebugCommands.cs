using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugCommands : MonoBehaviour
{
    //public bool DisableUI = false;
    public KeyCode DisableUIKey = KeyCode.F2;
    public LayerMask UILayers;
    private List<GameObject> _UIGameObjects = new();
    private bool _UIIsActive = true;

    private void Start()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            if (InLayerMask(UILayers, root.layer)) _UIGameObjects.Add(root);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(DisableUIKey)) ToggleUI();
    }

    private void ToggleUI()
    {
        _UIIsActive = !_UIIsActive;
        foreach (var root in _UIGameObjects) root.SetActive(_UIIsActive);
    }

    private bool InLayerMask(LayerMask layerMask, int layer) { return (layerMask.value & (1 << layer)) != 0; }
}
