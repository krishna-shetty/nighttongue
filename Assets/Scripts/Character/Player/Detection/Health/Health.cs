using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] protected int MaxHealth = 5;
    [SerializeField] private float MaxIFrameSecs = 1f;

    protected int CurrentHealth;
    protected float CurrentIFrames; // subtracts with delta time
    protected bool Invulnerable;

    public event Action<GameObject, GameObject, bool> OnDamage; // this, source, takes knockback
    public event Action<GameObject> OnDeath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentIFrames > 0)
        {
            CurrentIFrames -= Time.deltaTime;
            if (CurrentIFrames <= 0)
            {
                CurrentIFrames = 0;
                Invulnerable = false;
            }
        }
    }

    /// <summary>
    /// Returns true if damage was successfully dealt
    /// </summary>
    public virtual bool ApplyDamage(int damage, GameObject source,
        float overrideIFrames = -1f, bool knockback = false)
    {
        if (overrideIFrames == -1f && CheckInvulnerable(source)) return false;
        CurrentIFrames = overrideIFrames != -1f ? overrideIFrames : MaxIFrameSecs;
        Invulnerable = (CurrentIFrames > 0);

        SubtractDamage(damage, source);
        OnDamage?.Invoke(gameObject, source, knockback);
        if (CurrentHealth <= 0) Die();
        return true;
    }

    protected virtual void Die()
    {
        // Debug.Log(name + " died");
        InvokeOnDeath();

        // temp for testing
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when attempting to take damage.
    /// Overridable, returns true if invulnerable for any reason
    /// </summary>
    protected virtual bool CheckInvulnerable(GameObject source)
    {
        return Invulnerable || CurrentIFrames > 0;
    }

    protected void SubtractDamage(int damage, GameObject source)
    {
        CurrentHealth -= damage;
        // Debug.Log(name + " took " + damage + " damage :: health = " + CurrentHealth);
    }

    public void InvokeOnDeath()
    {
        OnDeath?.Invoke(gameObject);
    }

    public bool IsInvulnerable() { return Invulnerable; }

    /// <summary>
    /// For if behavior logic wants to give invulnerability for any reason
    /// </summary>
    public void SetInvulnerable(bool inv) { Invulnerable = inv; }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public float GetMaxIFrames() { return MaxIFrameSecs; }

    public float GetCurrentIFrames() { return CurrentIFrames; }

    public int GetCurrentHealth()
    {
        return CurrentHealth;
    }
}
