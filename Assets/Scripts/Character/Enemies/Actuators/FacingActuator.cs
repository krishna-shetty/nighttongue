using UnityEngine;

public class FacingActuator : IActuator
{
    private IDirectionInfo _externalDirectionCtx;
    public FacingActuator(IDirectionInfo externalDirectionCtx)
    {
        _externalDirectionCtx = externalDirectionCtx;
    }

    public void Act(IContext context)
    {
        if (_externalDirectionCtx.TargetDirection.HasValue)
        {
            Vector3 targetDirection = _externalDirectionCtx.TargetDirection.Value;
            Vector3 currentScale = context.TransformPosition.localScale;
            if (targetDirection.x <= 0) // to the left
            {
                Vector3 newScale = new Vector3(-1, currentScale.y, currentScale.z);
                context.TransformPosition.localScale = newScale;
            }
            else // x > 0, to the right
            {
                Vector3 newScale = new Vector3(1, currentScale.y, currentScale.z);
                context.TransformPosition.localScale = newScale;
            }
        }
    }
}
