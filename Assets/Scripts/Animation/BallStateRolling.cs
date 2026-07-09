using UnityEngine;
using System.Collections;
public class BallStateRolling : MonoBehaviour
{
    [SerializeField] private Transform ballVisual;
    [SerializeField] private PlayerController controller;
    [SerializeField] private GameObject vfxAfterBall;
    private TongueTransformHandler _transformHandler;
    private bool isBallState = false;
    [Header("parameter")]
    private float radius = 1f;
    //[SerializeField] private float moveSpeed = 6f; 
    private GameObject vfxInstance;
    private float minInput = 0.01f;
    private float backOffset = 0.65f;
    private float downOffset = 0.9f;
    private ParticleSystem[] particleSystems;
    [Header("direction")]
    [SerializeField] private Vector3 rollAxis = Vector3.forward;
    [SerializeField] private Transform facingReference;
    /*[SerializeField] private bool invertDirection = false;*/
    private void Awake()
    {
        _transformHandler = GetComponentInParent<TongueTransformHandler>();
    }

    private void OnEnable()
    {
        isBallState = true;

        if (_transformHandler != null)
            _transformHandler.OnTransformStateChanged += HandleTransformChanged;
    }

    private void OnDisable()
    {
        if (_transformHandler != null)
            _transformHandler.OnTransformStateChanged -= HandleTransformChanged;
        isBallState = false;
        ForceStopVFX();
    }

    private void HandleTransformChanged(TongueTransformEventArgs args)
    {
        isBallState = args.IsTransformed;

        if (!isBallState)
        {
            ForceStopVFX();
        }
    }

    private void ForceStopVFX()
    {
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
            vfxInstance = null;
            particleSystems = null;
        }
    }
    private void Start()
    {
        InitVFX();
    }

    private void InitVFX()
    {
        if (vfxAfterBall == null)
        {
            Debug.LogWarning("vfxAfterBall is null");
            return;
        }

        if (facingReference == null)
        {
            facingReference = transform;
        }

       /* vfxInstance = Instantiate(vfxAfterBall);
        particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>(true);

        StopVFX();
        UpdateVFXPosition();*/
    }

    private void LateUpdate()
    {
        if (ballVisual == null || controller == null || radius <= 0f)
            return;

        float v = controller.Velocity[0];
        /*
                if (Mathf.Abs(input) < minInput)
                    return;*/
        //Debug.Log("VelocityX: " + v);
        float distance = v * Time.deltaTime;
        float angle = (distance / radius) * Mathf.Rad2Deg;
        float sign = 1f;

        ballVisual.Rotate(rollAxis, angle * sign, Space.Self);

        
        UpdateVFXState();
        UpdateVFXPosition();
    }

    private IEnumerator FadeOutAndDestroy(GameObject vfx)
    {
        if (vfx == null) yield break;
        ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (var ps in systems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        float duration = 0.5f;
        float time = 0f;
        var renderers = vfx.GetComponentsInChildren<Renderer>();

        while (time < duration)
        {
            float alpha = 1f - (time / duration);
            foreach (var r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(vfx);
    }

    private void UpdateVFXPosition()
    {
        if (vfxInstance == null || controller == null)
            return;

        float dir = 1f;

        if (controller.Velocity[0] > minInput)
        {
            dir = 1f;
        }
        else if (controller.Velocity[0] < -minInput)
        {
            dir = -1f;
        }

        Vector3 back = -dir * Vector3.right * backOffset;
        Vector3 pos = transform.position + back + Vector3.down * downOffset;

        vfxInstance.transform.position = pos;

        Vector3 scale = vfxInstance.transform.localScale;
        scale.z = Mathf.Abs(scale.z) * dir;
        vfxInstance.transform.localScale = scale;
    }

    private void UpdateVFXState()
    {
        if (controller == null)
            return;

        float horizontalSpeed = Mathf.Abs(controller.Velocity[0]);
        //bool isMoving = horizontalSpeed > 0f;
        bool isMoving = horizontalSpeed > minInput;
        bool shouldShow = isMoving && isBallState;
        if (shouldShow)
        {
            if (vfxInstance == null)
            {
                vfxInstance = Instantiate(vfxAfterBall);
                vfxInstance.transform.localScale *= 2f;
                //particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>(true);
                particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>(true);
                UpdateVFXPosition();
            }
        }
        else
        {
            /* if (vfxInstance != null)
             {
                 Destroy(vfxInstance);
                 vfxInstance = null;
                 particleSystems = null;
             }*/
            if (vfxInstance != null)
            {
                StartCoroutine(FadeOutAndDestroy(vfxInstance));
                vfxInstance = null;
                particleSystems = null;
            }
        }
    }

    private void PlayVFX()
    {
        if (particleSystems == null) return;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (!particleSystems[i].isPlaying)
            {
                particleSystems[i].Play();
            }
        }
    }

    private void StopVFX()
    {
        if (particleSystems == null) return;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].isPlaying)
            {
                particleSystems[i].Stop();
            }
        }
    }

    private void StopVFXImmediate()
    {
        if (particleSystems == null) return;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void OnDestroy()
    {
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }
}