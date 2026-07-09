using UnityEngine;

public class CollisionInfo
{
    public Vector3 collisionDisplacement;
    public bool collisionHasHit;

    public CollisionInfo(Vector3 displacement, bool hasHit)
    {
        this.collisionDisplacement = displacement;
        this.collisionHasHit = hasHit;
    }
}
