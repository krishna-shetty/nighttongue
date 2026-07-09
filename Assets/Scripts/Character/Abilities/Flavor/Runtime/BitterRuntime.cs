using System;
using UnityEngine;


public class BitterRuntime : PassiveFlavorRuntimeBase
{
    public override EFlavor FlavorType => EFlavor.Bitter;
    private PlayerController _playerController;

    public BitterRuntime(
        FlavorManager flavorManager,
        BitterPresenter presenter,
        BitterFlavorSO data
    ) : base(
        flavorManager,
        presenter,
        new BitterModel(data.Duration),
        data
    )
    {
        Debug.Log("BitterRuntime :: Constructor called!");
        _playerController = flavorManager.GetComponent<PlayerController>();

        if (_playerController == null)
            Debug.LogWarning("BitterRuntime: PlayerController not found on FlavorManager GameObject!");
    }

    public override void OnActivate()
    {
        base.OnActivate();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
    }
}

