using System.Collections;
using UnityEngine;

public class FireCycleController : MonoBehaviour
{
    [Header("Targets to toggle")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider[] colliders;

    [Tooltip("Whole VFX GameObjects to toggle on/off")]
    [SerializeField] private GameObject[] vfxObjects;

    [Tooltip("Any MonoBehaviour that should be off during rest")]
    [SerializeField] private Behaviour[] damageBehaviours;

    [Header("Cycle Settings")]
    [SerializeField] private bool startFiring = true;

    [SerializeField] private float fireTime = 2f;
    [SerializeField] private float restTime = 2f;

    [Header("Randomize Fire/Rest")]
    [SerializeField] private bool randomizeFireTime = false;
    [SerializeField] private Vector2 fireTimeRange = new Vector2(1f, 3f);
    [SerializeField] private ParticleSystem[] particleSystems;

    [SerializeField] private bool randomizeRestTime = false;
    [SerializeField] private Vector2 restTimeRange = new Vector2(1f, 3f);

    [Header("Start Offset (Desync)")]
    [SerializeField] private bool restBeforeFirstFire = false;
    [SerializeField] private float initialRestTime = 0f;

    [SerializeField] private bool randomizeInitialRest = false;
    [SerializeField] private Vector2 initialRestRange = new Vector2(0f, 2f);

    [SerializeField] private bool randomizePhaseOffset = false;
    [SerializeField] private Vector2 phaseOffsetRange = new Vector2(0f, 2f);

    private Coroutine cycleRoutine;
    private bool isFiring;

    private void Reset()
    {
        AutoFillTargets();
    }

    private void Awake()
    {
        if ((renderers == null || renderers.Length == 0) &&
            (colliders == null || colliders.Length == 0))
        {
            AutoFillTargets();
        }
        
        if (particleSystems == null || particleSystems.Length == 0)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
    }

    private void OnEnable() => StartCycle();

    private void OnDisable() => StopCycle();

    [ContextMenu("Auto Fill Targets")]
    private void AutoFillTargets()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
    }

    public void StartCycle()
    {
        if (cycleRoutine != null) return;
        cycleRoutine = StartCoroutine(CycleLoop());
    }

    public void StopCycle()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }
    }

    private IEnumerator CycleLoop()
    {
        isFiring = startFiring;
        ApplyState(isFiring);

        if (restBeforeFirstFire && startFiring)
        {
            ApplyState(false);
            float d = randomizeInitialRest ? RandomInRangeSafe(initialRestRange) : Mathf.Max(0f, initialRestTime);
            yield return new WaitForSeconds(d);
            ApplyState(true);
            isFiring = true;
        }
        else if (randomizePhaseOffset)
        {
            float d = RandomInRangeSafe(phaseOffsetRange);
            yield return new WaitForSeconds(d);
        }

        while (true)
        {
            float onDuration = randomizeFireTime ? RandomInRangeSafe(fireTimeRange) : Mathf.Max(0f, fireTime);
            float offDuration = randomizeRestTime ? RandomInRangeSafe(restTimeRange) : Mathf.Max(0f, restTime);

            if (isFiring)
            {
                yield return new WaitForSeconds(onDuration);
                isFiring = false;
                ApplyState(false);
            }
            else
            {
                yield return new WaitForSeconds(offDuration);
                isFiring = true;
                ApplyState(true);
            }
        }
    }

    private void ApplyState(bool firing)
    {
        if (renderers != null)
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) renderers[i].enabled = firing;

        if (colliders != null)
            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i] != null) colliders[i].enabled = firing;

        if (firing)
        {
            if (vfxObjects != null)
                for (int i = 0; i < vfxObjects.Length; i++)
                    if (vfxObjects[i] != null) vfxObjects[i].SetActive(true);

            if (particleSystems != null)
                for (int i = 0; i < particleSystems.Length; i++)
                    if (particleSystems[i] != null) particleSystems[i].Play(true);
        }
        else
        {
            if (particleSystems != null)
                for (int i = 0; i < particleSystems.Length; i++)
                    if (particleSystems[i] != null)
                        particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
    
            StartCoroutine(DisableVfxWhenDone());
        }

        if (damageBehaviours != null)
            for (int i = 0; i < damageBehaviours.Length; i++)
                if (damageBehaviours[i] != null) damageBehaviours[i].enabled = firing;
    }

    private float RandomInRangeSafe(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);
        return Mathf.Max(0f, Random.Range(min, max));
    }
    
    private IEnumerator DisableVfxWhenDone()
    {
        if (particleSystems != null)
        {
            bool anyAlive = true;

            while (anyAlive)
            {
                anyAlive = false;

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    var ps = particleSystems[i];
                    if (ps != null && ps.IsAlive(true))
                    {
                        anyAlive = true;
                        break;
                    }
                }

                yield return null;
            }
        }

        if (vfxObjects != null)
            for (int i = 0; i < vfxObjects.Length; i++)
                if (vfxObjects[i] != null) vfxObjects[i].SetActive(false);
    }
}