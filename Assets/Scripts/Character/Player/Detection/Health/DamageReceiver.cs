using System;
using UnityEngine;

/// <summary>
/// Use this with hurtboxes to route damage to the same main health component.
/// </summary>
[RequireComponent(typeof(AkGameObj))]
public class DamageReceiver : MonoBehaviour
{
    public Health HealthComponent;
    public bool CanBeStomped = true;
    public bool damageImmune = false;

    [SerializeField, Tooltip("Triggered when damage causes loss of health")] private AK.Wwise.Event ReceiveDamageSound;
    [SerializeField, Tooltip("Triggered when damage does not cause loss of health")] private AK.Wwise.Event BlockDamageSound;

    public static event Action<int, GameObject> OnAnyDamageTaken;

    public bool ApplyDamage(int damage, GameObject source,
        float overrideIFrames = -1f, bool knockback = false)
    {
        if (HealthComponent == null)
            return false;

        bool didDamage = HealthComponent.ApplyDamage(damage, source, overrideIFrames, knockback);

        if (!didDamage)
        {
            // Audio
            if (BlockDamageSound != null && BlockDamageSound.IsValid()) { BlockDamageSound.Post(gameObject);}
            
            return false;
        }
            

        OnAnyDamageTaken?.Invoke(damage, this.gameObject);
        
        // Audio
        if (ReceiveDamageSound != null && ReceiveDamageSound.IsValid()) { ReceiveDamageSound.Post(gameObject);}
        
        return true;
    }
}
