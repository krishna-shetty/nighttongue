using UnityEngine;

public class DirectionSensor : ISensor
{

    private float _viewDistance = 5f;
    private Transform _target;

    public DirectionSensor(float viewDistance, Transform target)
    {
        this._viewDistance = viewDistance;
        this._target = target;
    }


    public void Sense(IContext info)
    {
        if (info is IDirectionInfo directionInfo)
        {
            if (_target != null)
            {
                Vector3 direction = _target.position - directionInfo.TransformPosition.position;
                if (direction.magnitude <= _viewDistance)
                {
                    directionInfo.TargetDirection = direction.normalized;
                }
                else
                {
                    directionInfo.TargetDirection = null;
                }
            }
            else
            {
                directionInfo.TargetDirection = null;
            }
        }
    }
}