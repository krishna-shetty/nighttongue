using UnityEngine;
using UnityEngine.Events;

public class IngredientDropoff : MonoBehaviour
{
    [Tooltip("Where to move the ingredients to")]
    public Transform DropoffStart;

    [Header("Event subscriptions")]
    public UnityEvent OnDropoff;

    public bool AllowRepeatDropoffs = false;
    private bool _hasDroppedOff = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasDroppedOff && !AllowRepeatDropoffs) return;

        var boat = other.GetComponent<BossBoat>(); // only use colliders from the main boat object
        var boatIngUser = other.GetComponentInChildren<BoatIngredientUser>();
        if (boat && boatIngUser)
        {
            if (boatIngUser.HeldIngredients.Count == 0) return;

            var offset = DropoffStart.position - boatIngUser.HeldIngredients[0].transform.position;
            offset.z = 0;

            foreach (GameObject ing in boatIngUser.HeldIngredients)
            {
                ing.transform.SetParent(null, true);
                ing.transform.position += offset;
                if (ing.GetComponent<Rigidbody>() is Rigidbody r) r.isKinematic = false;
            }

            _hasDroppedOff = true;
            OnDropoff.Invoke();
        }
    }
}
