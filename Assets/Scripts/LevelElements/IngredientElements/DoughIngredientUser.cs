using UnityEngine;

public class DoughIngredientUser : IngredientUser
{
    [Header("Dough specific fields")]
    public GameObject DoughObject;
    public Dough DoughFlattenScript;
    public IngredientType FinishedDough;

    public void Start()
    {
        if (!DoughFlattenScript) Debug.LogError("DoughIngredientUser :: Please set Dough in the inspector");
    }

    protected override bool TryAddIngredient(Bucket bucket)
    {
        if (!DoughFlattenScript.IsFlattened()) return false;
        return base.TryAddIngredient(bucket);
    }

    protected override void OnIngredientsFulfilled()
    {
        base.OnIngredientsFulfilled();
        var bucket = DoughObject.GetComponent<Bucket>();
        if (!bucket) bucket = DoughObject.AddComponent<Bucket>();
        bucket.SetHeldIngredient(FinishedDough);
    }
}
