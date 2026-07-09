using UnityEngine;

public class WindVolume : MonoBehaviour, IForceSource
{
    [SerializeField] private WindVolumeSO _volume;
    [SerializeField] private ParticleSystem particleEffect;
    public bool isActive = true;

    private int _directionSign = 1;
    private float _timer;
    private bool _isOneDirectional;

    void Start()
    {
        _isOneDirectional = (_volume.FirstHalfCycleTime <= 0f) || (_volume.LastHalfCycleTime <= 0f);

        if(_isOneDirectional)
        {
            _directionSign = (_volume.FirstHalfCycleTime > 0f) ? 1 : -1;
        }
        //ConfigureParticleSystem();
        //particleEffect.Play();
    }
    void Update()
    {
        if (_isOneDirectional)
            return;

        _timer += Time.deltaTime;

        float targetTime = (_directionSign == 1)
            ? _volume.FirstHalfCycleTime
            : _volume.LastHalfCycleTime;

        if (_timer >= targetTime)
        {
            _directionSign *= -1;
            _timer = 0f;
            UpdateParticleSystem();
        }
    }

    private void ConfigureParticleSystem()
    {
        if (particleEffect == null) return;

        Vector3 dir = _volume.Direction.normalized * _directionSign;
        particleEffect.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        // get bounds
        Bounds bounds;
        //if (TryGetComponent<MeshRenderer>(out var renderer))
        //    bounds = renderer.bounds;
        if (TryGetComponent<MeshCollider>(out var meshCol))
            bounds = meshCol.bounds;
        else
            bounds = new Bounds(transform.position, Vector3.one);

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        // compute offset so emitter sits at entry face of the wind volume
        Vector3 offset = Vector3.zero;

        if (Mathf.Abs(dir.x) > 0.5f)
            offset.x = -Mathf.Sign(dir.x) * extents.x;
        else if (Mathf.Abs(dir.y) > 0.5f)
            offset.y = -Mathf.Sign(dir.y) * extents.y;
        else if (Mathf.Abs(dir.z) > 0.5f)
            offset.z = -Mathf.Sign(dir.z) * extents.z;

        particleEffect.transform.position = center + offset;

        // configure particle system lifetime, speed, and shape for this and all children
        ParticleSystem[] systems = particleEffect.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.loop = _isOneDirectional;
            main.startSpeed = _volume.ForceStrength / 3.5f;

            float travelDistance = Mathf.Abs(dir.x) * (extents.x / 2f) +
                                    Mathf.Abs(dir.y) * (extents.y * 2f) +
                                    Mathf.Abs(dir.z) * (extents.z);

            float travelTime = (main.startSpeed.constant > 0f)
                ? travelDistance / main.startSpeed.constant
                : 0.1f;

            main.startLifetime = travelTime;

            // Match Shape scale to entry face of wind volume
            var shape = ps.shape;
            Vector3 size = bounds.size;

            // flatten axis along wind dir
            if (Mathf.Abs(dir.x) > 0.5f) size.x = 0.01f;
            if (Mathf.Abs(dir.y) > 0.5f) size.y = 0.01f;
            if (Mathf.Abs(dir.z) > 0.5f) size.z = 0.01f;

            // Apply correction factor for Y when wind is horizontal
            if (Mathf.Abs(dir.y) < 0.5f)
            {
                size.y *= (10f / 15f);  // ~0.67 correction scale
            }

            shape.scale = size;
        }
    }

    private void UpdateParticleSystem()
    {
        //particleEffect.Stop();
        //ConfigureParticleSystem();
        //particleEffect.Play();
    }

    public Vector3 GetForce()
    {
        if (isActive)
        {
            return _volume.Direction.normalized * _directionSign * _volume.ForceStrength;
        }
        else
        {
            return new Vector3(0, 0, 0);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IForceReceiver>(out var receiver))
        {
            receiver?.RegisterForceSource(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IForceReceiver>(out var receiver))
        {
            receiver?.UnregisterForceSource(this);
        }
    }

    public void ParticleSystemEnable()
    {
        //particleEffect.Play();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void ParticleSystemDisable()
    {
        //particleEffect.Stop();
        foreach (Transform child in transform)
        {
            Debug.Log(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }

}
