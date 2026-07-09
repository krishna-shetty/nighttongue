using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FlavorPresenterPair
{
    public EFlavor flavor;
    public MonoBehaviour presenter;
}

public interface IFlavorUIContext
{
    T GetFlavorPresenter<T>(EFlavor flavor) where T : MonoBehaviour;
}

public class FlavorUIRegistry : MonoBehaviour, IFlavorUIContext
{
    [SerializeField] private List<FlavorPresenterPair> _flavorPresenters;
    private Dictionary<EFlavor, MonoBehaviour> _flavorPresentersMap;

    private void Awake()
    {
        _flavorPresentersMap = new Dictionary<EFlavor, MonoBehaviour>();
        
        foreach (FlavorPresenterPair pair in _flavorPresenters)
        {
            _flavorPresentersMap[pair.flavor] = pair.presenter;
        }
    }
    public T GetFlavorPresenter<T>(EFlavor flavor) where T : MonoBehaviour
    {
        return _flavorPresentersMap.TryGetValue(flavor, out var p) ? p as T : null;
    }
}
