using System.Collections.Generic;
using UnityEngine;

public class BoatIngredientUser : IngredientUser
{
    [Header("Boat related stuff")]
    public GameObject BoatBucketPrefab; // needs to not have rigidbody
    public Transform BucketSpawnStart;
    public float SpawnPosXIncrement = 2f;

    private Vector3 _currentSpawnLocation;
    private bool _hasDough = false;

    [HideInInspector] public List<GameObject> HeldIngredients; // gameobj refs used for dropoffs

    private void Start()
    {
        _currentSpawnLocation = BucketSpawnStart.position;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!_hasDough && other.GetComponentInChildren<Dough>()) AddDough(other);
        else base.OnTriggerEnter(other);
    }

    protected override bool TryAddIngredient(Bucket bucket)
    {
        var item = bucket.HeldIngredient;
        if (!base.TryAddIngredient(bucket)) return false;

        // spawn bucket
        var newBucket = Instantiate(BoatBucketPrefab, gameObject.transform);
        newBucket.transform.position = _currentSpawnLocation;
        newBucket.GetComponent<Bucket>().SetHeldIngredient(item);
        if (newBucket.GetComponent<Rigidbody>() is Rigidbody r) r.isKinematic = true;

        HeldIngredients.Add(newBucket);
        OnAddIngredient.Invoke();
        _currentSpawnLocation.x += SpawnPosXIncrement;
        return true;
    }

    private void AddDough(Collider other)
    {
        _hasDough = true;
        foreach (Transform child in transform) Destroy(child.gameObject);
        HeldIngredients.Clear();

        // move it to boat
        var dough = other.gameObject;
        dough.transform.position = BucketSpawnStart.position;
        dough.transform.SetParent(transform, true);
        HeldIngredients.Add(other.gameObject);

        if (dough.GetComponent<Rigidbody>() is Rigidbody r) r.isKinematic = true;

        // update current/required ingredients
        if (dough.GetComponent<Bucket>() is Bucket bucket)
        {
            var item = bucket.HeldIngredient;
            if (!RequiredIngredients.Contains(item)
                || CurrentIngredients.Contains(item))
                return;

            CurrentIngredients.Add(item);
            if (RequiredIngredients.Count == CurrentIngredients.Count)
                OnIngredientsFulfilled();
        }
    }
}
