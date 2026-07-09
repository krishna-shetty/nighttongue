using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ConditionalMovingPlatform : MovingPlatform
{
    [SerializeField]
    public bool CanMove = false;

    public UnityEvent OnBlocked;
    public UnityEvent OnUnblocked;

    public void SetCanMove()
    {
        CanMove = true;
    }
    public void SetCanNotMove()
    {
        CanMove = false;
    }

    protected void MoveOnCondition()
    {
        if (CanMove)
        {
            Move();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveOnCondition();
    }
}
