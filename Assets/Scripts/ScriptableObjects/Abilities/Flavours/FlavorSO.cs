using UnityEngine;
using System.Collections.Generic;

public abstract class FlavorSO : ScriptableObject
{
    public string FlavorName;
    public string Description;
    public abstract EFlavor Flavor { get; }
    public List<AbilitySO> Abilities;
}
