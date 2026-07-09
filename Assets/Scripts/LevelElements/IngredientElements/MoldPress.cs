using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class MoldPress : MonoBehaviour
{
    public float TimeToFlatten = 5f;
    public float MinVelocityMagnitude = 0.75f;

    public GameObject ObjectToSpawnOnFinish;
    public Transform SpawnedObjectLocation;
    // public Animator Animator;

    private GameObject _player;
    private PlayerController _controller;
    private AbilityUser _abilityUser;

    private float _progress = 0f;
    private bool _isFinished = false;

    public UnityEvent OnMoldPressFinish;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.Find("Player");
        _controller = _player.GetComponent<PlayerController>();
        _abilityUser = _player.GetComponent<AbilityUser>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isFinished
            && (other.CompareTag("Player") || other.transform.IsChildOf(_player.transform))
            && _abilityUser.GetActiveAbility() is TongueTransformSO
            && math.length(_controller.Velocity) > MinVelocityMagnitude)
        {
            _progress = Mathf.Clamp01(_progress + Time.deltaTime / TimeToFlatten);
            // todo: sync animation
            Debug.Log(_progress);

            if (_progress == 1f) OnFinish();
        }
    }

    private void OnFinish()
    {
        _isFinished = true;
        if (ObjectToSpawnOnFinish)
        {
            var parent = SpawnedObjectLocation ? SpawnedObjectLocation.transform : gameObject.transform;
            var obj = Instantiate(ObjectToSpawnOnFinish, parent);
            obj.transform.SetPositionAndRotation(parent.position, parent.rotation);
        }
        OnMoldPressFinish?.Invoke();
    }
}
