using UnityEngine;

public class GroundSensor : ISensor
{
    private float _groundCheckDistance = 0.1f;
    private LayerMask _groundLayer;
    public GroundSensor(float groundCheckDistance, LayerMask groundLayer)
    {
        this._groundCheckDistance = groundCheckDistance;
        this._groundLayer = groundLayer;
    }

    public void Sense(IContext info)
    {
        if (info is IGroundInfo groundInfo)
        {
            groundInfo.IsGrounded = Physics.Raycast(groundInfo.TransformPosition.position, -groundInfo.TransformPosition.up, _groundCheckDistance, _groundLayer);
        }
    }
}
