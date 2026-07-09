using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T: Component
{
    private Queue<T> _pool = new Queue<T>();
    private T _objectPrefab;
    private Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _objectPrefab = prefab;
        _parent = parent;
        for(int i = 0; i < initialSize; i++)
        {
            T currObj = Object.Instantiate(prefab, parent);
            currObj.gameObject.SetActive(false);
            _pool.Enqueue(currObj);
        }
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        T currObj = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_objectPrefab, _parent);
        currObj.transform.SetPositionAndRotation(position, rotation);
        currObj.gameObject.SetActive(true);
        return currObj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
