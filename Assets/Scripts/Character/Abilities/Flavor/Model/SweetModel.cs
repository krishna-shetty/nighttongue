using System;
using UnityEngine;

public class SweetModel : ActiveModelBase
{
    public float CurrentOverflow { get; private set; }
    public float MaxOverflow { get; private set; }

    public bool Overcharged => CurrentOverflow > 0;

    public event Action<float> OnOverflowChanged;
    public event Action OnOverflowStarted;
    public event Action OnOverflowEnded;

    public SweetModel(float maxBattery) : base(maxBattery) { }

    public void ApplyOverflow(float overflow)
    {
        MaxOverflow = Mathf.Max(overflow, 0f);
        CurrentOverflow = MaxOverflow;
        OnOverflowStarted?.Invoke();
    }

    public override void ConsumeBattery(float amount)
    {
        if (Overcharged)
        {
            float prevOverflow = CurrentOverflow;
            CurrentOverflow = Mathf.Max(CurrentOverflow - amount, 0f);
            OnOverflowChanged?.Invoke(CurrentOverflow / MaxOverflow);

            if (prevOverflow > 0f && Mathf.Approximately(CurrentOverflow, 0f))
                OnOverflowEnded?.Invoke();
        }
        else
        {
            base.ConsumeBattery(amount);
        }
    }
}
