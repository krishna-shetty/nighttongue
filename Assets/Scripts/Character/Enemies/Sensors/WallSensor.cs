using UnityEngine;

public class WallSensor : ISensor
{
    private float _wallCheckDistance;
    private LayerMask _wallLayer;
    public WallSensor(float wallCheckDistance, LayerMask wallLayer)
    {
        this._wallCheckDistance = wallCheckDistance;
        this._wallLayer = wallLayer;
    }

    public void Sense(IContext info)
    {
        if(info is IWallInfo wallInfo)
        {
            wallInfo.IsTouchingRightWall = Physics.Raycast(wallInfo.TransformPosition.position, wallInfo.TransformPosition.right, _wallCheckDistance, _wallLayer);
            wallInfo.IsTouchingLeftWall = Physics.Raycast(wallInfo.TransformPosition.position, -wallInfo.TransformPosition.right, _wallCheckDistance, _wallLayer);
        }
        
    }
}
