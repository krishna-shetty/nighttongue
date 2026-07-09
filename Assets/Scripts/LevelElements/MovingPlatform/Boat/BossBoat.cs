using UnityEngine;

public class BossBoat : ConditionalMovingPlatform
{
    public bool isDestroyed = false;
    private WindVolume blockingWind;
    private bool blockedByWind = false;

    // Update is called once per frame
    void Update()
    {
        if (isDestroyed)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.CompareTag("Player"))
                {
                    Debug.Log("Boat died destroying player");
                    child.gameObject.GetComponent<DamageReceiver>().ApplyDamage(5000, gameObject);
                }
            }
        }
        if (blockedByWind == true && blockingWind.isActive == false)
        {
            CanMove = true;
            blockedByWind = false;
            OnUnblocked?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<WindVolume>(out WindVolume blockingObject))
        {
            if (blockingObject.isActive)
            {
                blockingWind = blockingObject;
                if (!blockedByWind) OnBlocked?.Invoke();
                blockedByWind = true;
                CanMove = false;
            }
        }
    }
}
