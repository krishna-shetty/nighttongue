using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ContextPair
{
    [Tooltip("Full type name, e.g. Namespace.IFlavorUIContext")]
    public string typeName;
    public MonoBehaviour contextObject;
}

public class UIContextProvider : MonoBehaviour
{
    public static UIContextProvider Instance { get; private set; }

    [SerializeField] private List<ContextPair> _contextsList = new();
    private Dictionary<Type, object> _contextsMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _contextsMap = new Dictionary<Type, object>();

        foreach (var pair in _contextsList)
        {
            var type = Type.GetType(pair.typeName);
            if (type == null)
            {
                Debug.LogWarning($"Cannot find type '{pair.typeName}' for UI context.");
                continue;
            }

            if (!_contextsMap.ContainsKey(type))
                _contextsMap[type] = pair.contextObject;
            else
                Debug.LogWarning($"Duplicate context for type '{pair.typeName}'");
        }
    }

    public T GetContext<T>() where T : class
    {
        var type = typeof(T);
        if (_contextsMap.TryGetValue(type, out var ctx))
            return ctx as T;

        Debug.LogWarning($"No UI context found for type '{type}'");
        return null;
    }
}
