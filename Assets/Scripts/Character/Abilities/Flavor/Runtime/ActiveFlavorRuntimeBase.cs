using System;
using UnityEngine;

public abstract class ActiveFlavorRuntimeBase : IFlavorRuntime
{
    public abstract EFlavor FlavorType { get; }
    public FlavorManager FlavorManager { get; }

    public event Action<IFlavorRuntime> OnExpired;

    protected ActiveModelBase _model;
    protected ActiveFlavorPresenterBase _presenter;
    protected ActiveFlavorSO _data;

    protected bool _isDraining;

    protected ActiveFlavorRuntimeBase(
        FlavorManager flavorManager,
        ActiveFlavorPresenterBase presenter,
        ActiveModelBase model,
        ActiveFlavorSO data
    )
    {
        FlavorManager = flavorManager;
        _presenter = presenter;
        _model = model;
        _presenter?.BindModel(_model);
        _data = data;
        _model.OnBatteryEmpty += HandleBatteryEmpty;
    }

    public virtual void OnActivate() => _presenter?.Activate();
    public virtual void OnDeactivate() => _presenter?.Deactivate();
    public virtual void OnAbilityActivated() => _isDraining = true;
    public virtual void OnAbilityCanceled() => _isDraining = false;

    public void Tick(float deltaTime)
    {
        //if (_isDraining)
        //{
        //    _model.ConsumeBattery(_data.ConsumptionRate * deltaTime);
        //    _presenter?.SetBattery(_model.CurrentBattery / _model.MaxBattery);
        //}
    }

    public void HandleBatteryEmpty()
    {
        OnExpired?.Invoke(this as IFlavorRuntime);
        this.OnDeactivate();
    }

}
