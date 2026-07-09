using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    [SerializeField] private float _stopDuration = 0.1f;
    [SerializeField] private float _stopScale = 0.0f; // lower values = bigger freeze
    [SerializeField] private PlayerHealth _health;

    private bool _isStopping;
    private float _initialTimeScale;
    private float _initialFixedDeltaTime;

    private void Awake()
    {
        _initialTimeScale = Time.timeScale;
        _initialFixedDeltaTime = Time.fixedDeltaTime;
        if (_health) _health.OnDamage += TriggerStop;
    }

    // Call this from your OnDamage event
    public void TriggerStop(GameObject victim, GameObject source, bool knockback)
    {
        if (!_isStopping) StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        Debug.Log("sadf");
        _isStopping = true;
        _initialTimeScale = Time.timeScale;
        _initialFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = _stopScale;
        Time.fixedDeltaTime = _initialFixedDeltaTime * Time.timeScale;
        yield return new WaitForSecondsRealtime(_stopDuration);

        Time.timeScale = _initialTimeScale;
        Time.fixedDeltaTime = _initialFixedDeltaTime;
        _isStopping = false;
    }
}
