using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Dough : MonoBehaviour
{
    [Header("Dough specific fields")]
    public GameObject DoughObject;
    public Vector2 TargetScale;
    public float TimeToFlatten = 5f;
    public float MinVelocityMagnitude = 0.75f;

    private GameObject _player;
    private PlayerController _controller;
    private AbilityUser _abilityUser;

    private Vector3 _targetScale;
    private Vector3 _originalScale;

    private float _progress = 0f;
    private bool _isFlattened = false;
    private bool _isFlattening = false;

    public UnityEvent OnFlattenFinish;
    public UnityEvent OnFlattenResume;
    public UnityEvent OnFlattenPause;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!DoughObject) Debug.LogError("Dough :: Please set the dough object in the inspector");

        _originalScale = DoughObject.transform.localScale;
        _targetScale = new(TargetScale.x, TargetScale.y, _originalScale.z);

        _player = GameObject.Find("Player");
        _controller = _player.GetComponent<PlayerController>();
        _abilityUser = _player.GetComponent<AbilityUser>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isFlattened
            && (other.CompareTag("Player") || other.transform.IsChildOf(_player.transform))
            && _abilityUser.GetActiveAbility() is TongueTransformSO
            && math.length(_controller.Velocity) > MinVelocityMagnitude)
        {
            _progress = Mathf.Clamp01(_progress + Time.deltaTime / TimeToFlatten);
            DoughObject.transform.localScale = Vector3.Lerp(_originalScale, _targetScale, _progress);

            if (!_isFlattening)
            {
                _isFlattening = true;
                OnFlattenResume?.Invoke();
            }

            if (_progress == 1f)
            {
                _isFlattened = true;
                _isFlattening = false;
                OnFlattenFinish?.Invoke();
                ArrowPointer.Instance.UpdateToNextGoal();
            }
        }
        else _isFlattening = false;
    }

    public bool IsFlattened() { return _isFlattened; }
}
