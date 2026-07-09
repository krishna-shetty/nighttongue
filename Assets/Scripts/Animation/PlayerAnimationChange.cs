using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class PlayerAnimationChange : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Animator ballAnimator;
    [SerializeField] PlayerController controller;
    //[SerializeField] PlayerInputHandler _playerInputHandler;
    UniversalTongueHandler _universalHandler;
    TongueTransformHandler _tongueTransformHandler;
    InputManager _inputManager;
    FlavorManager _flavorManager;
    Health _health;
    // PlayerInputHandler _playerInputHandler;
    private PushOffOverhang _pushOffOverhang;
    AbilityUser _abilityUser;
    private bool wasKnockbackLastFrame = false;
    RigBuilder _rigBuilder;
    [SerializeField] string moveParam = "IsMoving";
    [SerializeField] string jumpParam = "IsJumping";
    [SerializeField] string facingParam = "Facing";
    [SerializeField] string turnLeftTrigger = "TurnLeft";
    [SerializeField] string turnRightTrigger = "TurnRight";
    [SerializeField] string turningParam = "IsTurning";
    [SerializeField] string jumpTrigger = "IsJump";
    [SerializeField] string FailedG = "FailedGrapple";
    [SerializeField] string Taste = "Taste";
    [SerializeField] string reactionTypeParam = "ReactionType";
    [SerializeField] string deathTrigger = "Death";
    [SerializeField] GameObject _base;
    [SerializeField] GameObject _transformed;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private GameObject vfxAfterSweet;
    //[SerializeField] private GameObject vfxAfterBall;
    [SerializeField] private SkinnedMeshRenderer tongueRenderer;
    [SerializeField] private Material sweetTongueMaterial;
    [SerializeField] private Material oldMaterial;
    [SerializeField] string hitTrigger = "Hit";
    [SerializeField] string headhitTrigger = "HeadHit";
    [SerializeField] private string normalDeathState = "Death";
    [SerializeField] private string specialDeathState = "SpecialDeath";
    [SerializeField, Range(0f, 1f)] private float specialDeathChance = 0.05f;


    //[SerializeField] string landTrigger = "IsLAND";
    [SerializeField] private TongueController tongue;

    //[SerializeField] float enter = 0.05f, exit = 0.02f;


    [Header("Flip Settings")]
    [SerializeField] Transform flipModel;


    int hash;
    bool moving;
    bool facingRight = true;
    private bool jumping = false;
    private bool wasGrounded;
    private bool grounded;
    private bool isSwinging = false;
    private bool _isTransformed = false;
    private bool isSweetActive = false;
    private bool isDead = false;
    bool hasPendingFacing;
    bool pendingFacingRight;

    void Awake()
    {
        _universalHandler = GetComponentInParent<UniversalTongueHandler>();
        _tongueTransformHandler = GetComponentInParent<TongueTransformHandler>();
        _inputManager = InputManager.Instance;
        _flavorManager = GetComponentInParent<FlavorManager>();
        _rigBuilder = GetComponentInChildren<RigBuilder>();
        _pushOffOverhang = GetComponent<PushOffOverhang>();
        _abilityUser = GetComponent<AbilityUser>();
        _health = GetComponent<Health>();
        if (_health == null)
            _health = GetComponentInParent<Health>();
        if (_health == null)
            _health = GetComponentInChildren<Health>();
        //if (!_playerInputHandler) _playerInputHandler = GetComponentInParent<PlayerInputHandler>();
    }

    void Reset()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!controller) controller = GetComponentInParent<PlayerController>();
        //if (!_playerInputHandler) _playerInputHandler = GetComponentInParent<PlayerInputHandler>();
        if (!flipModel && animator != null) flipModel = animator.transform;

    }

    void OnEnable()
    {
        if (controller != null)
        {
            controller.OnJumpStarted += HandleJumpStarted;
            controller.OnSwingStart += HandleSwingStarted;
            controller.OnSwingEnd += HandleSwingEnd;
            _universalHandler.FailedAnimationEvent += HandleFailGrapple;
            // _playerInputHandler.OnPickup += HandleTaste;
            _flavorManager.OnPickupFlavor += HandleNewFlavor;
            _tongueTransformHandler.OnTransformStateChanged += HandleBallRolling;
            _health.OnDeath += HandleDeath;
            _pushOffOverhang.OnHeadHit += HandleHeadHit;

        }



        animator.SetBool(facingParam, facingRight);
        animator.SetBool(turningParam, false);
        ApplyModelFacing();
    }

    void OnDisable()
    {
        if (controller != null)
        {
            controller.OnJumpStarted -= HandleJumpStarted;
            _universalHandler.FailedAnimationEvent -= HandleFailGrapple;
            controller.OnSwingStart -= HandleSwingStarted;
            controller.OnSwingEnd -= HandleSwingEnd;
            //_playerInputHandler.OnPickup -= HandleTaste;
            _flavorManager.OnPickupFlavor -= HandleNewFlavor;
            _tongueTransformHandler.OnTransformStateChanged -= HandleBallRolling;
            _health.OnDeath -= HandleDeath;
            _pushOffOverhang.OnHeadHit -= HandleHeadHit;

        }

    }




    void Update()
    {
        if (isDead) return;

        grounded = controller.GroundDetector && controller.GroundDetector.IsGrounded;
        if (grounded)
        {	// the length of the tongue while swinging is remembered as long as you're still up in the air...?			// as soon as you touch ground, reset the tongue length back to standard
            SwingingState.ResetLength();
        }
        if (isSwinging)
        {
            UpdateSwing();
        }
        float input = controller.HorizontalInput;
        bool hold = tongue != null && tongue.IsHold;
        bool attached = tongue != null && tongue.IsAttached;
        wasGrounded = grounded;

        if (!hold && !attached)
        {
            if (input > 0 && !facingRight) { RequestFacing(true); }
            else if (input < 0 && facingRight) { RequestFacing(false); }
        }

        bool moving = Mathf.Abs(input) > 0f && grounded;
        animator.SetBool("IsGrounded", grounded);

        animator.SetBool(moveParam, moving);
        animator.SetBool(jumpParam, jumping);
        ChangeMaterialBack();
        HitAnimation();

    }

    private void HandleFailGrapple()
    {

        animator.SetTrigger(FailedG);
    }

    private void HandleBallRolling(TongueTransformEventArgs args)
    {
        if (args.IsTransformed == true)
        {
            controller.RaiseStartBallAnimation();
            animator.Play("turning_ball");
            Debug.Log("Started ball transform animation");
        }
        else if (args.IsTransformed == false)
        {
            animator.Play("turning_ball_reverse");
        }
    }

    public void BallTurnAnimationFinished()
    {   /*controller.RaiseEndBallAbility(); // SFX
        controller.ResistsWind = true;
        _base.SetActive(false);
        _transformed.SetActive(true);
        _pushOffOverhang.enabled = false;

        _isTransformed = true;*/

        Debug.Log("BallTurnAnimationFinished called");

        if (_tongueTransformHandler.IsTransformed) return;
        _tongueTransformHandler.TurningBallHandler();


    }
    public void PlayVFX()
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("vfxPrefab is null");
            return;
        }

        float downOffset = 1f;
        float backOffset = 0.5f;

        Vector3 forward = flipModel.forward;

        Vector3 spawnPos =
            transform.position
            - forward * backOffset
            + Vector3.down * downOffset;

        /*GameObject vfx = Instantiate(vfxPrefab, spawnPos, Quaternion.identity);

        ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.simulationSpeed = 2.5f;
        }*/
        GameObject vfx = Instantiate(vfxPrefab, spawnPos, Quaternion.identity);

        ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);

        float maxLifeTime = 0f;

        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.simulationSpeed = 2.5f;

            float duration = main.duration + main.startLifetime.constantMax;
            if (duration > maxLifeTime)
            {
                maxLifeTime = duration;
            }
        }

        Destroy(vfx, maxLifeTime);
    }

    private void HandleNewFlavor(FlavorSO flavor)
    {
        if (flavor == null) return;

        if (flavor is SaltyFlavorSO)
        {
            return;
        }

        if (flavor.Abilities.Count > 0 && flavor.Abilities[0] is TongueTransformSO)
            return;

        animator.SetTrigger(Taste);

        if (flavor is SweetFlavorSO)
            animator.SetInteger(reactionTypeParam, 1);
        // else if (flavor is SaltyFlavorSO)
        //     animator.SetInteger(reactionTypeParam, 2);
        else
            animator.SetInteger(reactionTypeParam, 0);
    }

    public void SweetVFX()
    {
        //Debug.Log("111 SweetVFX called");

        if (vfxAfterSweet == null)
        {
            Debug.LogWarning("vfxAfterSweet is null");
            return;
        }

        Vector3 vfxPosition = transform.position + Vector3.up * 1.5f;

        GameObject vfx = Instantiate(vfxAfterSweet, vfxPosition, Quaternion.identity);


        ParticleSystem[] systems = vfx.GetComponentsInChildren<ParticleSystem>(true);

        float maxLifeTime = 0f;

        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;

            float duration = main.duration + main.startLifetime.constantMax;

            if (duration > maxLifeTime)
            {
                maxLifeTime = duration;
            }
        }


        Destroy(vfx, maxLifeTime);
    }

    private void HandleDeath(GameObject deadObject)
    {
        Debug.Log("PlayerAnimationChange.HandleDeath called");

        isDead = true;

        animator.ResetTrigger(hitTrigger);
        animator.SetBool(moveParam, false);
        animator.SetBool(jumpParam, false);
        animator.SetBool(turningParam, false);
        //ScreenFader.Instance.FadeOut(1f);
        string deathStateToPlay =
        UnityEngine.Random.value < specialDeathChance
        ? specialDeathState
        : normalDeathState;

        Debug.Log($"Death animation selected: {deathStateToPlay}");

        animator.Play(deathStateToPlay, 0, 0f);
        DeathCircleFader.Instance.StartDeathFade(transform);
    }


    private void HandleJumpStarted()
    {
        bool canJumpForAnimation =
        controller.GroundDetector != null &&
        (controller.GroundDetector.IsGrounded || controller.HasCoyoteBuffered);

        if (!canJumpForAnimation) return;
        animator.SetTrigger(jumpTrigger);

    }

    private void HandleSwingStarted()
    {
        isSwinging = true;
        UpdateSwing();
    }

    private void HandleSwingEnd()
    {
        isSwinging = false;
        UpdateSwing();
    }


    private void HandleHeadHit()
    {
        animator.SetTrigger(headhitTrigger);
    }
    void UpdateSwing()
    {
        if (isSwinging)
        {
            if (facingRight)
                animator.SetInteger("SwingDir", 1);
            else
                animator.SetInteger("SwingDir", -1);
        }
        else
        {
            animator.SetInteger("SwingDir", 0);
        }
    }

    void RequestFacing(bool right)
    {
        if (!grounded)
        {
            facingRight = right;
            hasPendingFacing = false;

            animator.SetBool(facingParam, facingRight);
            animator.SetBool(turningParam, false);


            animator.ResetTrigger(turnLeftTrigger);
            animator.ResetTrigger(turnRightTrigger);

            ApplyModelFacing();
            return;
        }

        if (animator.GetBool(turningParam)) return;
        if (facingRight == right) return;


        hasPendingFacing = true;
        pendingFacingRight = right;

        animator.SetBool(turningParam, true);

        if (right) animator.SetTrigger(turnRightTrigger);
        else animator.SetTrigger(turnLeftTrigger);


    }
    public void TurnAnimationFinished()
    {
        if (!hasPendingFacing)
        {
            animator.SetBool(turningParam, false);
            return;
        }

        facingRight = pendingFacingRight;
        hasPendingFacing = false;
        ApplyModelFacing();

        animator.SetBool(facingParam, facingRight);
        animator.SetBool(turningParam, false);
    }


    void ApplyModelFacing()
    {
        if (flipModel == null) return;
        Debug.Log($"flipModel={flipModel.name}");
        Vector3 rot = flipModel.localEulerAngles;
        rot.y = facingRight ? 90f : 180f;
        flipModel.localEulerAngles = rot;
        Debug.Log($"Applied model facing: {rot.y}");
    }

    public void MaterialChange()
    {
        if (tongueRenderer != null && sweetTongueMaterial != null)
        {
            tongueRenderer.material = sweetTongueMaterial;
        }
    }

    private void HitAnimation()
    {
        if (isDead) return;

        if (_health != null && _health.GetCurrentHealth() <= 0)
            return;

        bool isKnockbackNow = controller.IsInState<KnockbackState>();

        if (isKnockbackNow && !wasKnockbackLastFrame)
        {
            animator.SetTrigger(hitTrigger);
        }

        wasKnockbackLastFrame = isKnockbackNow;
    }

    private void MaterialReset()
    {
        if (tongueRenderer != null)
        {
            tongueRenderer.material = oldMaterial;
        }
    }

    private void ChangeMaterialBack()
    {
        int currentReaction = animator.GetInteger(reactionTypeParam);

        if (currentReaction == 1 && !isSweetActive)
        {
            isSweetActive = true;
        }
        else if (currentReaction != 1 && isSweetActive)
        {
            isSweetActive = false;
            MaterialReset();
        }
    }



    public void CircleFaded()
    {
        StartCoroutine(DeathEndFlow());
        //LevelManager.Instance.RestartScene();
    }

    private IEnumerator DeathEndFlow()
    {
        yield return DeathCircleFader.Instance.CloseCircleToBlack(transform);
        LevelManager.Instance.RestartScene();

    }
}