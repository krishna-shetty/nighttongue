using UnityEngine;

public class WindVolumeTriggerForwarder : MonoBehaviour, IForceReceiver
{
    [SerializeField] private IForceReceiver _forwardTo;

    private void Awake()
    {
        IForceReceiver[] receivers = GetComponentsInParent<IForceReceiver>(true);

        foreach (var r in receivers)
        {
            if (r != this)
            {
                _forwardTo = r;
                break;
            }
        }

        Debug.Log(_forwardTo);

        if (_forwardTo == null)
            Debug.LogWarning($"{name} couldn’t find a parent IForceReceiver to forward to.");
    }

    public void RegisterForceSource(IForceSource source)
    {
        _forwardTo.RegisterForceSource(source);
    }

    public void UnregisterForceSource(IForceSource source)
    {
        _forwardTo.UnregisterForceSource(source);
    }
}
