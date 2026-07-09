using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IngredientUser : MonoBehaviour
{
    public List<IngredientType> RequiredIngredients;
    public List<IngredientType> CurrentIngredients;

    [Header("Behavior on ingredients fulfilled (leave empty if none)")]
    public Animator Animator;
    public string AnimationStateNameToPlay;

    [Tooltip("Use a prefab, and make sure to set an animation event that calls OnAnimationFinished()")]
    public GameObject ObjectToSpawnAfterAnimation;
    public Transform SpawnedObjectLocation;

    [Tooltip("Use nonzero value to spawn object after a timer, instead of waiting for the animation")]
    public float TimeToSpawnObject = 0f;

    [Header("Event subscriptions")]
    public UnityEvent OnHasIngredients;
    public UnityEvent OnFinishedAnimation;
    public UnityEvent OnFinishedTimer;
    public UnityEvent OnAddIngredient;

    private bool _isTimerCounting = false;

    private void Start()
    {
        if (!Animator) Animator = GetComponent<Animator>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Bucket bucket = other.GetComponent<Bucket>();
        if (bucket) TryAddIngredient(bucket);
    }

    protected virtual bool TryAddIngredient(Bucket bucket)
    {
        // note this will not work if recipe uses multiple units of the same ingredient (i.e. eggs twice). tweak to use structs if necessary
        var item = bucket.HeldIngredient;
        if (!RequiredIngredients.Contains(item)
            || CurrentIngredients.Contains(item))
            return false;

        CurrentIngredients.Add(item);
        OnAddIngredient.Invoke();
        bucket.RemoveIngredient();
        if (RequiredIngredients.Count == CurrentIngredients.Count)
            OnIngredientsFulfilled();

        return true;
    }

    protected virtual void OnIngredientsFulfilled()
    {
        //Debug.Log("ingredientuser :: do something");
        if (Animator && AnimationStateNameToPlay.Length > 0) Animator.Play(AnimationStateNameToPlay);
        OnHasIngredients.Invoke();

        if (TimeToSpawnObject != 0f)
        {
            var timer = Mathf.Max(TimeToSpawnObject, 0f);
            _isTimerCounting = true;
        }
    }

    protected virtual void OnAnimationFinished()
    {
        //Debug.Log("ingredientuser :: animationfinished");
        if (ObjectToSpawnAfterAnimation && TimeToSpawnObject == 0f)
            SpawnObject();
        OnFinishedAnimation.Invoke();
    }

    private void SpawnObject()
    {
        var parent = SpawnedObjectLocation ? SpawnedObjectLocation.transform : gameObject.transform;
        var obj = Instantiate(ObjectToSpawnAfterAnimation, parent);
        obj.transform.SetPositionAndRotation(parent.position, parent.rotation);
    }

    private void Update()
    {
        if (_isTimerCounting)
        {
            TimeToSpawnObject -= Time.deltaTime;
            if (TimeToSpawnObject <= 0f)
            {
                OnFinishedTimer.Invoke();
                _isTimerCounting = false;
                SpawnObject();
            }
        }
    }
}
