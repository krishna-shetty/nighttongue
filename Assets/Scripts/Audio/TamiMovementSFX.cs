using System;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(AkGameObj))]
public class TamiMovementSFX : MonoBehaviour
{

    private PlayerController _playerController;
    private FlavorManager _flavorManager;
    private TongueController _tongueController;
    private UniversalTongueHandler _tongueHandler;

    [SerializeField] private float velocityMagnitude;
    [SerializeField] private float swingPreBottomAngleDegrees = 15f;
    
    
    private bool _firstLandSfxPlayed = false;
    private bool _swingLowSfxPlayed = false;

    private bool _isBall = false;
    private bool _swinging = false;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _flavorManager = GetComponent<FlavorManager>();
        _tongueController = GetComponentInChildren<TongueController>();
        _tongueHandler = GetComponent<UniversalTongueHandler>();
        
        _firstLandSfxPlayed = false;
    }
    private void OnEnable()
    {
        _playerController.OnJump += PlayJump;
        _playerController.OnSwingStart += PlaySwingAttach;
        _playerController.OnSwingEnd += PlaySwingDetach;
        _playerController.OnSwingForward += PlaySwingForward;
        _playerController.OnSwingBackward += PlaySwingBackward;
        _playerController.OnSwingAscend += PlaySwingAscend;
        _playerController.OnSwingDescend += PlaySwingDescend;
        _playerController.OnSwingNeutral += PlaySwingNeutral;
        _playerController.OnSwingMin += PlaySwingMin;
        _playerController.OnSwingMax += PlaySwingMax;
        _playerController.OnBallStart += PlayBallStart;
        _playerController.OnBallStartAnimation += PlayBallCurl;
        _playerController.OnBallEnd += PlayBallRelease;
        _playerController.OnGroundedStart += PlayLand;
        _playerController.OnGrappleEnd += PlaySwingDetach;
        //_playerController.OnTongueDragAttach += PlayTongueDragAttach;
        //_playerController.OnTongueDragDetach += PlaySwingDetach;
        
        _flavorManager.OnPickupFlavor += PickupFlavor;
        
        //_playerController.OnGrappleStart += PlaySwingAttach;
        
        _tongueController.OnTongueExtension += PlayTongueExtend;
        //_tongueController.OnTongueAttach += PlayTongueAttach;
        _tongueController.OnTongueRetraction += PlayTongueRetract;
        _tongueController.OnTongueExtendToLength += PlayTongueExtend;

        _tongueHandler.FailedAnimationEvent += PlayTongueMove;
    }

    private void OnDisable()
    {
        _playerController.OnJump -= PlayJump;
        _playerController.OnSwingStart -= PlaySwingAttach;
        _playerController.OnSwingEnd -= PlaySwingDetach;
        _playerController.OnSwingForward -= PlaySwingForward;
        _playerController.OnSwingBackward -= PlaySwingBackward;
        _playerController.OnSwingAscend -= PlaySwingAscend;
        _playerController.OnSwingDescend -= PlaySwingDescend;
        _playerController.OnSwingNeutral -= PlaySwingNeutral;
        _playerController.OnSwingMin -= PlaySwingMin;
        _playerController.OnSwingMax -= PlaySwingMax;
        _playerController.OnBallStart -= PlayBallStart;
        _playerController.OnBallStartAnimation -= PlayBallCurl;
        _playerController.OnBallEnd -= PlayBallRelease;
        _playerController.OnGroundedStart -= PlayLand;
        _playerController.OnGrappleEnd -= PlaySwingDetach;
        //_playerController.OnTongueDragAttach -= PlayTongueDragAttach;
        //_playerController.OnTongueDragDetach -= PlaySwingDetach;
        
        _flavorManager.OnPickupFlavor -= PickupFlavor;
        
        //_playerController.OnGrappleStart -= PlaySwingAttach;
        
        _tongueController.OnTongueExtension -= PlayTongueExtend;
        //_tongueController.OnTongueAttach -= PlayTongueAttach;
        _tongueController.OnTongueRetraction -= PlayTongueRetract;

        _tongueHandler.FailedAnimationEvent -= PlayTongueMove;
    }

    void Update()
    {
        AkUnitySoundEngine.SetRTPCValue("Velocity_X", _playerController.Velocity.x, null);
        
        if (_swinging)
        {
            velocityMagnitude = math.lengthsq(_playerController.Velocity);
            AkUnitySoundEngine.SetRTPCValue("Velocity_Magnitude_Peak", velocityMagnitude, null);
            AkUnitySoundEngine.SetRTPCValue("Velocity_Magnitude", velocityMagnitude, null);
            var angleFromHorizontal = Mathf.Abs(Vector2.SignedAngle(Vector2.right, _playerController.Velocity));
            if (angleFromHorizontal <= swingPreBottomAngleDegrees)
            {
                if (!_swingLowSfxPlayed)
                {
                    _swingLowSfxPlayed = true;
                    AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_swish", gameObject);
                }
            }
            else
            {
                _swingLowSfxPlayed = false;
            }
        }
    }
    
    private void PlayJump()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_jump", gameObject);
    }
    
    private void PlayTongueAttach(Vector3 target, bool swinging)
    {
        if (swinging) return;
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_attach", gameObject);
    }

    private void PlaySwingAttach()
    {
        _swinging = true;
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_attach", gameObject);
    }
    
    private void PlaySwingDetach()
    {
        _swinging = false;
        _swingLowSfxPlayed = false;
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_detach", gameObject);
    }

    private void PlaySwingForward()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_forward", gameObject);
    }

    private void PlaySwingBackward()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_backward", gameObject);
    }
    private void PlaySwingAscend()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_ascend", gameObject);
    }
    private void PlaySwingDescend()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_descend", gameObject);
    }

    private void PlaySwingNeutral()
    {
        AkUnitySoundEngine.PostEvent("Stop_SFX_tami_swing_vertical", gameObject);
    }

    private void PlaySwingMin()
    {
        AkUnitySoundEngine.PostEvent("Stop_SFX_tami_swing_vertical", gameObject);
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_min", gameObject);
    }
    private void PlaySwingMax()
    {
        AkUnitySoundEngine.PostEvent("Stop_SFX_tami_swing_vertical", gameObject);
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_max", gameObject);
    }

    private void PlayBallStart() // Happens when ball animation finishes
    {
        _isBall = true;
    }

    private void PlayBallCurl() // happens when ball state is triggered
    {
        AkUnitySoundEngine.SetState("AbilityState", "Ball");
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_ballroll", gameObject);
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_ballcurl", gameObject);
    }

    private void PlayBallRelease()
    {
        _isBall = false;
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_ballrelease", gameObject);
        
        AkUnitySoundEngine.PostEvent("Stop_SFX_tami_ballroll", gameObject);
        
        AkUnitySoundEngine.SetState("AbilityState", "Default");
    }

    private void PlayLand()
    {
        if (!_firstLandSfxPlayed)
        {
            _firstLandSfxPlayed = true;
            return;
        }
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_land", gameObject);
    }
    
    private void PickupFlavor(FlavorSO flavor)
    {
        switch (flavor.Flavor)
        {
            case (EFlavor.Bitter):
                AkUnitySoundEngine.PostEvent("Play_SFX_pickup_bitter", gameObject);
                break;
            case (EFlavor.Salty):
                AkUnitySoundEngine.PostEvent("Play_SFX_pickup_sour", gameObject);
                break;
            case (EFlavor.Sweet):
                AkUnitySoundEngine.PostEvent("Play_SFX_pickup_sweet", gameObject);
                break;
            default:
                break;
        }
    }

    private void PlayTongueExtend(Vector3 target, float duration)
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_ascend", gameObject);
    }

    private void PlayTongueExtend()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_ascend", gameObject);
    }
    private void PlayTongueRetract(float duration)
    {
        uint id;
        id = AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_descend", gameObject);
        AkUnitySoundEngine.StopPlayingID(id, Mathf.CeilToInt(1000*duration));
    }

    private void PlayTongueDragAttach()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_swing_attach", gameObject);
        AkUnitySoundEngine.PostEvent("Stop_SFX_tami_swing_vertical", gameObject);
    }

    // Eventually attach this sound to the animator instead to align it better
    private void PlayTongueMove()
    {
        AkUnitySoundEngine.PostEvent("Play_SFX_tami_moveTongue", gameObject);
    }
}
