using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class StompDamageDealer : DamageDealer
{
    [SerializeField] private PlayerController Controller;

    [SerializeField, Tooltip("Max angle deviation (degrees) from the downwards vertical that still counts as a stomp instead of a normal collision.")]
    private float MaxStompAngle;
    
    [SerializeField, Tooltip("Targets that can be bounced on via stomp")]
    private LayerMask BounceTargets;
    

    private void Start()
    {
        OnDamageDealt += HandleStomp;
    }

    protected override void OnCollisionEnter(Collision collision) => base.OnCollisionEnter(collision);

    protected override void OnCollisionStay(Collision collision) => base.OnCollisionStay(collision);

    protected override void OnTriggerEnter(Collider other) => base.OnTriggerEnter(other);

    protected override void OnTriggerStay(Collider other) => base.OnTriggerStay(other);

    // move this subscription to controller?
    private void HandleStomp(GameObject source, GameObject target)
    {
        if (!InBounceLayerMask(target.layer)) return;
        Debug.Log("got through");
        Controller.RequestedJump = true;
        Controller.HasCoyoteBuffered = true;
    }

    protected override void TryHit(Collider other)
    {
        var dmgReceiver = CheckHit(other);
        if (dmgReceiver)
        {
            PlayDealDamageSound();
            if (other.GetComponent<DamageReceiver>().damageImmune)
            {
                OnDamageDealt.Invoke(gameObject, other.gameObject);
            }
            else
            {
                bool didDamage = dmgReceiver.ApplyDamage(DamageAmount, gameObject, overrideIFrames: 0.1f);
                if (didDamage) OnDamageDealt?.Invoke(gameObject, other.gameObject);
            }
        }
    }

    protected override DamageReceiver CheckHit(Collider other)
    {
        var dmgReceiver = base.CheckHit(other);
        bool stompable = CheckStomp(other.gameObject);
        return (Controller && stompable) ? dmgReceiver : null;
    }

    // Note: stomp checks only the first collider on other, else defaults to the object's transform
    // (but there should only be one collider per object anyways)
    public bool CheckStomp(GameObject other)
    {
        if (!other) return false;
        var collider = other.GetComponent<Collider>();

        // check falling/jump state
        var moveState = Controller.GetCurrentState();
        if (moveState is not FallingState && moveState is not JumpingState) return false;

        // check direction
        Vector3 playerPos = new(transform.position.x, transform.position.y, 0);
        Vector3 otherPos = collider ? collider.transform.position : other.transform.position;
        otherPos.z = 0f;
        Vector3 collisionDir = (otherPos - playerPos).normalized; // from player to other
        Debug.Log(other.gameObject + " :: collisionDir = " + collisionDir + " :: angleToDown = " + Vector3.Angle(collisionDir, Vector3.down));
        return Vector3.Angle(collisionDir, Vector3.down) <= MaxStompAngle;
    }

    private bool InBounceLayerMask(int layer) { return (BounceTargets.value & (1 << layer)) != 0; }
}
