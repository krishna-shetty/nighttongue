using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Handles all movement step logic that modifies velocity and position.
/// </summary>
/// <remarks>
/// (Mostly complete, try to avoid adding anything here unless you REALLY need to add a new logic function.)
/// </remarks>
public abstract class MovementHandlerBase
{
    protected PlayerController Context;
    protected CharacterController Cc;
    protected PlayerCollisionHandler CollisionHandler;
    protected float stateMultiplier = 1f;

    public MovementHandlerBase(PlayerController controller)
    {
        Context = controller;
        Cc = Context.GetComponent<CharacterController>();
        CollisionHandler = Context.GetComponent<PlayerCollisionHandler>();

        if (CollisionHandler == null)
        {
            Debug.LogWarning("MovementHandlerBase: PlayerCollisionHandler component not found on PlayerController.");
        }
    }

    public abstract void Apply(float gravity, MoveActionSO move);

    /// <summary>
    /// General multiplier multiplicatively stacks with x/y multipliers. Avoid setting both unless intended
    /// </summary>
    public virtual void ApplyExternalForces(float multiplier = 1f, float xMultiplier = 1f, float yMultiplier = 1f)
    {
        if (!Context) return;
        Vector3 force = Context.GetTotalExternalForce();
        force *= multiplier;
        Context.Velocity += new float2(force.x * xMultiplier, force.y * yMultiplier) * Time.fixedDeltaTime;
    }

    public void ApplyVerticalMovement(float gravity)
    {
        Context.Velocity.y += gravity * Time.fixedDeltaTime;
    }

    public void ApplyPrecisionMovement(MoveActionSO move)
    {
        float targetSpeed = Context.HorizontalInput * move.MaxHorizontalSpeed;
        Context.Velocity.x = targetSpeed;
    }

    public void ApplyMomentumMovement(MoveActionSO move)
    {
        float horizontalTargetSpeed = Context.HorizontalInput * move.MaxHorizontalSpeed;
        float currentSpeed = Context.Velocity.x;

        // Determine if player is counter-strafing
        bool isCounterStrafing = (Context.HorizontalInput > Context.Settings.FloatPrecisionThreshold && currentSpeed < 0)
                                 || (Context.HorizontalInput < -Context.Settings.FloatPrecisionThreshold && currentSpeed > 0);

        float acceleration = isCounterStrafing ? move.Deceleration : move.Acceleration;

        // Calculate intended speed change this frame
        float speedDifference = horizontalTargetSpeed - currentSpeed;
        float accelerationChange = acceleration * Time.fixedDeltaTime;

        if (Mathf.Abs(Context.HorizontalInput) > Context.Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Sign(currentSpeed) == Mathf.Sign(Context.HorizontalInput))
            {
                // Moving in same direction as input
                if (Mathf.Abs(currentSpeed) < Mathf.Abs(horizontalTargetSpeed))
                {
                    // Accelerate up to target speed
                    if (Mathf.Abs(speedDifference) <= accelerationChange)
                    {
                        Context.Velocity.x = horizontalTargetSpeed;
                    }
                    else
                    {
                        Context.Velocity.x += Mathf.Sign(speedDifference) * accelerationChange;
                    }
                }
                // Already at or above max input speed: do not add further input-based speed
            }
            else
            {
                // Counter-strafe to reverse direction
                if (Mathf.Abs(speedDifference) <= accelerationChange)
                {
                    Context.Velocity.x = horizontalTargetSpeed;
                }
                else
                {
                    Context.Velocity.x += Mathf.Sign(speedDifference) * accelerationChange;
                }
            }
        }
    }

    public void ApplySpeedDecay(MoveActionSO move)
    {
        float currentSpeed = Mathf.Abs(Context.Velocity.x);

        if (currentSpeed > move.MaxHorizontalSpeed)
        {
            float excessSpeed = currentSpeed - move.MaxHorizontalSpeed;
            float decayAmt = excessSpeed * move.SpeedDecayRate * Time.fixedDeltaTime;

            if (Context.Velocity.x > 0f)
            {
                Context.Velocity.x = Mathf.Max(Context.Velocity.x - decayAmt, move.MaxHorizontalSpeed);
            }
            else if (Context.Velocity.x < 0f)
            {
                Context.Velocity.x = Mathf.Min(Context.Velocity.x + decayAmt, -move.MaxHorizontalSpeed);
            }
        }
    }

    public void ApplyFriction(MoveActionSO move)
    {
        if (Mathf.Abs(Context.HorizontalInput) < Context.Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Abs(Context.Velocity.x) < Context.Settings.FloatPrecisionThreshold)
            {
                Context.Velocity.x = 0f;
            }
            else
            {
                float delta = move.GroundFrictionForce * Time.fixedDeltaTime;

                if (Context.Velocity.x > 0f)
                {
                    Context.Velocity.x = Mathf.Max(Context.Velocity.x - delta, 0f);
                }
                else if (Context.Velocity.x < 0f)
                {
                    Context.Velocity.x = Mathf.Min(Context.Velocity.x + delta, 0f);
                }
            }
        }
    }

    public void ClampVerticalVelocity(MoveActionSO move)
    {
        Context.Velocity.y = Mathf.Clamp(Context.Velocity.y, -move.MaxVerticalSpeed, move.MaxVerticalSpeed);
    }

    /// <remarks>
    /// Affects position
    /// </remarks>
    public void HandleOverhangPushOff()
    {
        if (Context.PushOff() != null)
        {
            Context.PushOff().TryPushOffLedge(Context.Velocity.y, Time.fixedDeltaTime);
        }
        else
        {
            Debug.LogWarning("PushOffOverhang component is missing on PlayerMovement object. Please add it to enable overhang push-off functionality.");
        }
    }

    /// <summary>
    /// Applies collision displacement (affects velocity and position)
    /// </summary>
    public void SimulateStep()
    {
        Vector3 velocity = new(Context.Velocity.x, Context.Velocity.y, 0f);
        Vector3 preResolutionVelocity = velocity;
        Vector3 preResolutionPosition = Context.transform.position;
        Vector3 motion = velocity * Time.fixedDeltaTime;

        // CharacterController handles collisions internally
        CollisionFlags flags = Cc.Move(motion);

        if (flags != CollisionFlags.None)
        {
            CollisionHandler.NotifyCollision();
        }

        Vector3 postResolutionPosition = Context.transform.position;
        Vector3 postResolutionVelocity = (postResolutionPosition - preResolutionPosition) / Time.fixedDeltaTime;

        Context.Velocity = new float2(postResolutionVelocity.x, postResolutionVelocity.y);
    }
}
