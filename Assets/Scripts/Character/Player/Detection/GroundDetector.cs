using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    private TongueTransformHandler _tongueTransformHandler;
    private bool _isTransformed = false;
    public bool IsGrounded { get; private set; }
    public bool JustLanded { get; private set; }
    public bool WasGrounded;
    public Vector3 GroundNormal => groundNormal;
    private CharacterController _cc;
    private Vector3 groundNormal = Vector3.up;
    private int lastCheckedFrame = -1;
    private int slopePushFrameCount = 0;
    private bool slopePushedRight = false;
    private bool slopePushedLeft = false;

    private void OnEnable()
    {
        _tongueTransformHandler = GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged += HandleTransformStateChange;
        }
    }

    private void OnDisable()
    {
        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformStateChange;
        }
    }

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
    }

    public bool IsOnSteepSlope()
    {
        // Ensure the character is grounded before performing the check
        if (_cc.isGrounded)
        {
            if (Time.frameCount != lastCheckedFrame)
            {   // we have to do a sphere cast to get the slope of the ground
                // don't do this more than once per frame
                lastCheckedFrame = Time.frameCount;
                if (Physics.SphereCast(transform.position, _cc.radius, Vector3.down, out RaycastHit hitInfo, _cc.height / 2f + 0.1f))
                {
                    groundNormal = hitInfo.normal;
                }
            }
            // Calculate the angle of the slope
            float angle = Vector3.Angle(Vector3.up, groundNormal);

            // Return true if the angle exceeds the slope limit
            return angle > _cc.slopeLimit;
        }
        groundNormal = Vector3.up; // Default to flat ground if not grounded or no hit
        return false;
    }

    public void Refresh()
    {
        WasGrounded = IsGrounded;

        IsGrounded = _cc.isGrounded;
        if (IsGrounded)
        {
            if (IsOnSteepSlope())
            {
                IsGrounded = false;
            }
        }
        else
        {   // not grounded
            groundNormal = Vector3.up;
        }

        if (slopePushedLeft && slopePushedRight)
        {   // stuck in a V shape between two slopes, treat as grounded to allow the player to jump out
            IsGrounded = true;
        }

        JustLanded = IsGrounded && !WasGrounded;
        slopePushFrameCount = 0;
        slopePushedRight = false;
        slopePushedLeft = false;
    }

    private void HandleTransformStateChange(TongueTransformEventArgs args)
    {
        _isTransformed = args.IsTransformed;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (slopePushFrameCount < 5)
        {   // do not do this more than 5 frames in a row... could get caught in an infinite loop.
            slopePushFrameCount++;
            if (hit.normal.y < Mathf.Sin(Mathf.Deg2Rad * _cc.slopeLimit))
            {
                Vector3 push = hit.normal;
                if (push.x < 0.0f)
                {
                    slopePushedLeft = true;
                }
                if (push.x > 0.0f)
                {
                    slopePushedRight = true;
                }
                //push.y = 0.0f;    // let's try allowing the full push
                push.z = 0.0f;
                _cc.Move(push * 0.01f); // Move the character slightly away from the wall to prevent sticking
            }
        }
    }
}