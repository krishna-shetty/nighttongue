using UnityEngine;

public class ScriptedGameObjectEvent : MonoBehaviour
{
    [Header("Event Type")]
    public ManageType EventType;
    public ActivationType ActiveType;
    private bool _eventActivated = false;

    [Header("Spawn Settings")]
    public GameObject SpawnObject;
    public Vector3 SpawnPosition;
    
    [Header("Destroy Settings")]
    public string DestroyName;

    public enum ManageType { Destroy, Spawn }
    public enum ActivationType { Trigger, Event}

    public void DestroyGameObjectByName()
    {
        GameObject gameObject = GameObject.Find(DestroyName);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void SpawnGameObject()
    {
        Instantiate(SpawnObject, SpawnPosition, Quaternion.identity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_eventActivated && ActiveType == ActivationType.Trigger)
        {
            ActivateEvent();
        }
    }

    public void ActivateEvent()
    {
        _eventActivated = true;
        if (EventType == ManageType.Spawn)
        {
            SpawnGameObject();
        }
        else
        {
            DestroyGameObjectByName();
        }
    }
}
