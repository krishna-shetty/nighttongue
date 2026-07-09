using System.Collections;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    [Tooltip("How long it takes to fill the bucket")]
    public float FillDelay = 0f;

    public IngredientType HeldIngredient;
    public IngredientSource CurrentSource = null;

    public void TryAddSource(IngredientSource source)
    {
        if (HeldIngredient || CurrentSource) return;
        CurrentSource = source;
        StartCoroutine(TryAddIngredient(source));
    }
    
    public void TryRemoveSource(IngredientSource source)
    {
        if (CurrentSource == source) CurrentSource = null;
    }

    private IEnumerator TryAddIngredient(IngredientSource source)
    {
        yield return new WaitForSeconds(FillDelay);
        if (CurrentSource == source) HeldIngredient = source.Ingredient;
    }

    public void SetHeldIngredient(IngredientType ingredient) { HeldIngredient = ingredient; }

    public void RemoveIngredient()
    {
        if (HeldIngredient) HeldIngredient = null;
        // temp
        gameObject.SetActive(false);
    }
}
