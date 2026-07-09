using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Add this extension to a CinemachineCamera (CM_Ball / CM_Capsule) to control
/// whether the camera follows the player's vertical movement (jumping).
/// When "Jump With Player" is off, the camera locks its Y position.
/// </summary>
[AddComponentMenu("Cinemachine/Extensions/Camera Jump Lock")]
public class CameraJumpLock : CinemachineExtension
{
    public bool JumpWithPlayer = true;

    float m_LockedY;
    bool m_HasLockedY;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        if (JumpWithPlayer)
        {
            // Track the current Y
            m_LockedY = state.RawPosition.y;
            m_HasLockedY = true;
            return;
        }

        // First frame with lock off — capture current Y
        if (!m_HasLockedY)
        {
            m_LockedY = state.RawPosition.y;
            m_HasLockedY = true;
        }

        // Override the Y position to stay locked
        var pos = state.RawPosition;
        pos.y = m_LockedY;
        state.RawPosition = pos;
    }
}
