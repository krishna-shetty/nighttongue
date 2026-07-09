using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private StompDamageDealer _stomp;

    private AbilityUser _abilities;

    private void Awake()
    {
        _abilities = GetComponent<AbilityUser>();
    }

    protected override bool CheckInvulnerable(GameObject source)
    {
        if (_abilities && _abilities.GetActiveAbility() is TongueTransformSO) return true;

        if (_stomp && _stomp.InLayerMask(source.layer) && _stomp.CheckStomp(source)) return true;

        return base.CheckInvulnerable(source);
    }

    protected override void Die()
    {
        InvokeOnDeath();

        //LevelManager.Instance.RestartScene();
    }
}
