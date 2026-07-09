using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class TurnEventRelay : MonoBehaviour
{
    public PlayerAnimationChange target;
    private InputSystem_Actions _controls;
    private bool _tasteLockActive;
    PlayerInputHandler _playerInputHandler;
    /* [SerializeField] private RigBuilder rigBuilder;
     [SerializeField] private TongueController tongue;*/
    void Awake()
    {
        _playerInputHandler = GetComponentInParent<PlayerInputHandler>();

    }

    void Reset()
    {
        if (target == null)
            target = GetComponentInParent<PlayerAnimationChange>();

        /*if (rigBuilder == null)
            rigBuilder = GetComponentInParent<RigBuilder>();*/

    }

    public void TurnAnimationFinished()
    {
        if (target != null)
            target.TurnAnimationFinished();
    }
    public void PlayVFX()
    {
        //Debug.Log("play vfx");
        if (target != null)
            target.PlayVFX();
    }

    public void SweetVFX()
    {
        if(target != null)
            target.SweetVFX();
    }

/*
    public void DeathAnimationFinished()
    {
        if (target != null)
        target.DeathAnimationFinished();
    }*/

    public void CircleFaded()
    {
       if (target != null)
       target.CircleFaded();
    }
    public void BallTurnAnimationFinished()
    {
        if (target != null)
            target.BallTurnAnimationFinished();
    }

   /* public void BallTurnBackAnimationFinished()
    {
        if (target != null)
            target.BallTurnBackAnimationFinished();
    }*/

    public void TasteLockInput()
    {
        if (_tasteLockActive) return;
        _tasteLockActive = true;
        if (_playerInputHandler != null) _playerInputHandler.SetFrozen(true);
    }

    public void TasteUnlockInput()
    {
        if (!_tasteLockActive) return;
        _tasteLockActive = false;
        if (_playerInputHandler != null) _playerInputHandler.SetFrozen(false);
    }

    public void MaterialChange()
    {
        if(target!= null)
            target.MaterialChange();
    }

    public void StartGrapple()
    {
        if (_playerInputHandler != null) _playerInputHandler.SetFrozen(true);
    }
    public void FinishFailGrapple()
    {
        if (_playerInputHandler != null) _playerInputHandler.SetFrozen(false);
    }

}
