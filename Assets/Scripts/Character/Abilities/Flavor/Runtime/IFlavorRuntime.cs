using System;
using UnityEngine;

public interface IFlavorRuntime
{
    EFlavor FlavorType { get; }
    FlavorManager FlavorManager { get; }

    void OnActivate();
    void OnDeactivate();
    void OnAbilityActivated();
    void OnAbilityCanceled();
    void Tick(float deltaTime);

    event Action<IFlavorRuntime> OnExpired;
}
