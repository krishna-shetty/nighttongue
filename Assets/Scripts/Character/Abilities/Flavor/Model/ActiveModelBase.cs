using System;
using UnityEngine;

public abstract class ActiveModelBase : IModel
{
    public float CurrentBattery { get; protected set; }
    public float MaxBattery { get; protected set; }

    public event Action<float> OnBatteryChanged;
    public event Action OnBatteryEmpty;

    protected ActiveModelBase(float maxBattery)
    {
        MaxBattery = maxBattery;
        CurrentBattery = maxBattery;
    }

    public virtual void ConsumeBattery(float amount)
    {
        float prevBattery = CurrentBattery;
        CurrentBattery = Mathf.Max(CurrentBattery - amount, 0f);
        OnBatteryChanged?.Invoke(GetNormalizedBattery());
        
        if (prevBattery > 0f && Mathf.Approximately(CurrentBattery, 0f))
        {
            OnBatteryEmpty?.Invoke();
        }
    }

    public float GetNormalizedBattery() => CurrentBattery / MaxBattery;
}
