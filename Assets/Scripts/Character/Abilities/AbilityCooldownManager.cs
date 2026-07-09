using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityCooldownManager
{
    private static Dictionary<AbilitySO, float> _abilityAvailableAt;

    /// <summary>
    /// Call to initialize/reinitialize the entire dictionary of when abilities are next available
    /// </summary>
    public static void InitializeAbilityAvailableAt(List<AbilitySO> abilities)
    {
        _abilityAvailableAt ??= new Dictionary<AbilitySO, float>();
        _abilityAvailableAt.Clear();

        foreach (var ability in abilities)
        {
            _abilityAvailableAt.Add(ability, 0f);
        }
    }

    /// <summary>
    /// Resets cooldown for ability if already in dictionary
    /// </summary>
    public static void AddAbility(AbilitySO ability)
    {
        _abilityAvailableAt[ability] = 0f;
    }

    public static void RemoveAbility(AbilitySO ability)
    {
        if (_abilityAvailableAt.ContainsKey(ability)) _abilityAvailableAt.Remove(ability);
    }

    /// <summary>
    /// Marks the next availablity as current time + cooldown
    /// </summary>
    public static void UpdateCooldown(AbilitySO ability, float cooldown)
    {
        if (!_abilityAvailableAt.ContainsKey(ability))
        {
            Debug.LogError("AbilityCooldownManager :: Attempted to update cooldown of unregistered ability.");
            return;
        }

        _abilityAvailableAt[ability] = Time.time + cooldown;
    }

    public static bool IsAvailable(AbilitySO ability)
    {
        if (!_abilityAvailableAt.ContainsKey(ability))
        {
            Debug.LogError("AbilityCooldownManager :: Attempted to update cooldown of unregistered ability.");
            return false;
        }

        return Time.time >= _abilityAvailableAt[ability];
    }
}
