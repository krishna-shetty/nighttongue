using System;
using UnityEngine;

public abstract class PassiveFlavorRuntimeBase : IFlavorRuntime
{
    public abstract EFlavor FlavorType { get; }
    public FlavorManager FlavorManager { get; }

    public event Action<IFlavorRuntime> OnExpired;

    protected PassiveModelBase _model;
    protected PassiveFlavorPresenterBase _presenter;
    protected PassiveFlavorSO _data;

    protected PassiveFlavorRuntimeBase(
        FlavorManager flavorManager,
        PassiveFlavorPresenterBase presenter,
        PassiveModelBase model,
        PassiveFlavorSO data
    )
    {
        FlavorManager = flavorManager;
        _presenter = presenter;
        _model = model;
        _presenter?.BindModel(_model);
        _data = data;
        _model.OnDurationCompleted += HandleDurationCompleted;
    }

    public virtual void OnActivate() => _presenter?.Activate();
    public virtual void OnDeactivate() => _presenter?.Deactivate();

    public virtual void OnAbilityActivated() { }
    public virtual void OnAbilityCanceled() { }

    public void Tick(float deltaTime)
    {
        _model.Tick(deltaTime);
    }

    public void HandleDurationCompleted()
    {
        OnExpired?.Invoke(this as IFlavorRuntime);
        this.OnDeactivate();
    }
}
