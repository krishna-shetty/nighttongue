using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class DamageDealer : MonoBehaviour
{
    [SerializeField] protected LayerMask Targets;
    [SerializeField] protected int DamageAmount;
    [SerializeField] protected bool IsActive = true;

    [Tooltip("Use if this object uses a collider and rigidbody (parent does not count)")]
    public bool UseCollision = false;

    [Tooltip("(Preferred) Use if this object wants to detect triggers, or uses a trigger with no rigidbody")]
    public bool UseTrigger = true;

    [SerializeField] protected bool DoesKnockback = true;
    [SerializeField] protected bool DamageOnStay = true;

    public Action<GameObject,GameObject> OnDamageDealt; // source, target
    
    [Header("Audio")]
    [SerializeField, Tooltip("The sound to play when this object attempts to damage object")]
    protected AK.Wwise.Event DealDamageSound;

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (UseCollision) TryHit(other.collider);
        Debug.Log("collision");
    }

    protected virtual void OnCollisionStay(Collision other)
    {
        if (UseCollision && DamageOnStay) TryHit(other.collider);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (UseTrigger) TryHit(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (UseTrigger && DamageOnStay) TryHit(other);
    }

    // Note: be careful of double hit if both OnCollision/Trigger are enabled and the object taking damage has no i-frames
    protected virtual void TryHit(Collider other)
    {
        var dmgReceiver = CheckHit(other);
        if (dmgReceiver)
        {
            PlayDealDamageSound();
            bool didDamage = dmgReceiver.ApplyDamage(DamageAmount, gameObject, knockback: DoesKnockback);
            if (didDamage || other.GetComponent<DamageReceiver>().damageImmune) { OnDamageDealt?.Invoke(gameObject, other.gameObject); }
        }
    }

    /// <returns>
    /// other's DamageReceiver, or null if failed check
    /// </returns>
    protected virtual DamageReceiver CheckHit(Collider other)
    {
        if (!IsActive || !InLayerMask(other.gameObject.layer)) return null;
        var dmgReceiver = other.gameObject.GetComponent<DamageReceiver>();
        if (!dmgReceiver || !dmgReceiver.enabled) return null;
        if (!dmgReceiver.CanBeStomped) return null;
        return dmgReceiver;
    }

    public bool InLayerMask(int layer) { return (Targets.value & (1 << layer)) != 0; }

    public void SetActive(bool active) { IsActive = active; }

    protected void PlayDealDamageSound()
    {
        if (DealDamageSound != null && DealDamageSound.IsValid())
            DealDamageSound.Post(gameObject);
    }
}
