using UnityEngine;

public class IngredientSource : MonoBehaviour
{
    public IngredientType Ingredient;

    private void OnTriggerEnter(Collider other)
    {
        Bucket bucket = other.GetComponent<Bucket>();
        if (bucket) bucket.TryAddSource(this);
    }

    private void OnTriggerExit(Collider other)
    {
        Bucket bucket = other.GetComponent<Bucket>();
        if (bucket) bucket.TryRemoveSource(this); // attempts to cancel ingredient fill if bucket leaves too soon
    }
}
