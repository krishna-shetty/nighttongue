using System;
using UnityEngine;

public abstract class PassiveModelBase : IModel
{
    public float TotalDuration { get; protected set; }
    public float ElapsedTime { get; protected set; }

    public event Action OnDurationCompleted;
    protected PassiveModelBase(float totalDuration)
    {
        TotalDuration = totalDuration;
        ElapsedTime = 0f;
    }

    public virtual void Tick(float deltaTime)
    {
        ElapsedTime += deltaTime;
        if (ElapsedTime >= TotalDuration)
        {
            OnDurationCompleted?.Invoke();
        }
    }

    public float GetNormalizedTime() => ElapsedTime / TotalDuration;
}
