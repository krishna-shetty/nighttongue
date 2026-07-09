using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbilitySO : ScriptableObject
{
    [FormerlySerializedAs("abilityName")] public string AbilityName;
    [FormerlySerializedAs("inputActionName")] public string InputActionName;
    [FormerlySerializedAs("icon")] public Sprite Icon;
    //public EFlavor Flavor = EFlavor.None;
    [SerializeField] public bool IsHoldAbility = true;

    public virtual void Activate(GameObject user) { }
    public virtual void OnPress(GameObject user) { }
    public virtual void OnHold(GameObject user) { }
    public virtual void OnRelease(GameObject user) { }
}
