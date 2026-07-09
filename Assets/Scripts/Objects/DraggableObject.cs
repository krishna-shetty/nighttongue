using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class DraggableObject : MonoBehaviour, IForceReceiver, IForceSource
{
    [SerializeField] public DraggablePropertiesSO Properties;
    private Rigidbody _rb;
    private readonly HashSet<IForceSource> _sources = new();
    private GameObject _blockedWindVolume;
    [SerializeField] private float _checkDistance = 2f;
    [SerializeField] private LayerMask _layer = 1 << 6;
    private Vector3 _groundNormal;
    private bool _wasGrounded = true;
    private float _previousXVelocity;
    public event Action OnLifted;
    [SerializeField] private AK.Wwise.Event[] _OnAttachWwiseEvents;
    [SerializeField] private AK.Wwise.Event[] _onDetachWwiseEvents;
    [SerializeField] private AK.Wwise.Event _onStartDragWwiseEvent;
    [SerializeField] private AK.Wwise.Event _onStopDragWwiseEvent;
    [SerializeField] private AK.Wwise.Event _onGroundedWwiseEvent;

    public Vector3 GetForce()
    {
        Vector3 n = _groundNormal;
        //Debug.Log("Ground normal: " + n);
        if (n == Vector3.zero) return Vector3.zero;

        Vector3 v = Vector3.ProjectOnPlane(_rb.linearVelocity, n);

        float N = Physics.gravity.magnitude * _rb.mass *
                  Mathf.Cos(Vector3.Angle(n, Vector3.up) * Mathf.Deg2Rad);

        float mu = Properties.Friction;   

        // kinetic friction magnitude
        float F = mu * N;

        // direction opposite motion
        Vector3 dir = -v.normalized;

        return dir * F;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _checkDistance);
    }

    private bool CheckGrounded(out RaycastHit hit)
    {
        Vector3 origin = transform.position;
        if (Physics.Raycast(origin, Vector3.down, out hit, _checkDistance, _layer))
        {
            return true;
        }
        return false;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            Debug.LogWarning("DraggableObject requires a Rigidbody to react to wind!");
    }

    public void RegisterForceSource(IForceSource source)
    {
        if (source is WindVolume && Properties.DragType == DraggableType.WindImmune)
            return;
        
        if (_sources.Contains(source)) _sources.Add(source);
    }
    public void UnregisterForceSource(IForceSource source)
    {
        if (source is WindVolume && Properties.DragType == DraggableType.WindImmune)
            return;

        if (_sources.Contains(source)) _sources.Remove(source);
    }

    private void FixedUpdate()
    {
        if (_rb == null || _sources.Count == 0) return;

        Vector3 total = Vector3.zero;
        foreach (var s in _sources)
            total += s.GetForce();

        _rb.AddForce(total, ForceMode.Force);
    }

    private void Update()
    {
        if (_blockedWindVolume != null && Properties != null && Properties.DragType == DraggableType.WindImmune)
        {
            HandleWindBlocking();
        }

        RaycastHit hit;
        bool grounded = CheckGrounded(out hit);

        if (!grounded && _wasGrounded)
        {
            Debug.Log("DraggableObject :: Invoking OnLifted");
            OnLifted?.Invoke();
        }

        if (grounded && !_wasGrounded)
        {
            PlayGroundedSound();
        }

        _wasGrounded = grounded;
        _groundNormal = grounded ? hit.normal : Vector3.zero;
    }

    private void HandleWindBlocking()
    {
        (float, float) objectSize = MeshSize.GetSize(gameObject);
        (float, float) windSize = MeshSize.GetSize(_blockedWindVolume);

        bool fullyCoversWind =
            (transform.position.x - objectSize.Item1 / 2) <= (_blockedWindVolume.transform.position.x - windSize.Item1 / 2) &&
            (transform.position.x + objectSize.Item1 / 2) >= (_blockedWindVolume.transform.position.x + windSize.Item1 / 2);

        var wind = _blockedWindVolume.GetComponent<WindVolume>();
        if (!wind) return;

        wind.enabled = !fullyCoversWind;
        wind.isActive = !fullyCoversWind;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<WindVolume>(out WindVolume wind))
        {
            _blockedWindVolume = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<WindVolume>(out WindVolume wind))
        {
            _blockedWindVolume = null;
            wind.enabled = true;
            wind.isActive = true;
            _blockedWindVolume = null;
        }
    }

    public void PlayAttachedSound()
    {
        if (_OnAttachWwiseEvents != null)
        {
            foreach (var i in _OnAttachWwiseEvents)
            {
                if (i.IsValid())
                {
                    i.Post(gameObject);
                }
            }
        }
        PlayStartDragSound();
    }
    
    public void PlayDetachedSound()
    {
        if (_onDetachWwiseEvents != null)
        {
            foreach (var i in _onDetachWwiseEvents)
            {
                if (i.IsValid())
                {
                    i.Post(gameObject);
                }
            }
        }
        PlayStopDragSound();
    }
    
    private void PlayGroundedSound()
    {
        if (_onGroundedWwiseEvent != null && _onGroundedWwiseEvent.IsValid())
        {
            _onGroundedWwiseEvent.Post(gameObject);
        }
    }
    
    private void PlayStartDragSound()
    {
        if (_onStartDragWwiseEvent != null && _onStartDragWwiseEvent.IsValid())
        {
            _onStartDragWwiseEvent.Post(gameObject);
        }
    }

    private void PlayStopDragSound()
    {
        if (_onStopDragWwiseEvent != null && _onStopDragWwiseEvent.IsValid())
        {
            _onStopDragWwiseEvent.Post(gameObject);
        }
    }
}
