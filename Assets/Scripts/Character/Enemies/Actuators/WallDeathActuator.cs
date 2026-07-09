using UnityEngine;

public class WallDeathActuator : IActuator
{
    private IWallInfo _externalWallCtx;
    private Health _enemyHealth;
    private GameObject _enemySource;
    public WallDeathActuator(IWallInfo extenalWallContext, Health enemyHealth, GameObject enemySource)
    {
        _externalWallCtx = extenalWallContext;
        _enemyHealth = enemyHealth;
        _enemySource = enemySource;
    }
    public void Act(IContext context)
    {
        if (context is IMoveInfo moveInfo)
        {
            if (_externalWallCtx.IsTouchingLeftWall || _externalWallCtx.IsTouchingRightWall)
            {
                _enemyHealth.ApplyDamage(9999, _enemySource);
            }
        }
    }
}
