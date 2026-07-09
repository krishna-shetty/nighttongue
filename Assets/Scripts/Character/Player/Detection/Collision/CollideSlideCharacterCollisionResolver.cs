using System;
using UnityEngine;

public class CollideSlideCharacterCollisionResolver
{
    [SerializeField] private int _maxCollideAndSlideDepth = 5;
    [SerializeField] private float _maxSlopeAngle = 55f; // Maximum slope angle to consider for sliding
    [SerializeField] private LayerMask _layer;
    [SerializeField] private float _skinWidth = 0.02f; // Default skin width

    public CollideSlideCharacterCollisionResolver(int maxDepth, float maxSlope, LayerMask layer, float skinWidth)
    {
        _maxCollideAndSlideDepth = maxDepth;
        _maxSlopeAngle = maxSlope;
        _layer = layer;
        _skinWidth = skinWidth;
    }


    //TODO: Check if magnitude needs to be calculated before projection
    private Vector3 ProjectAndScale(Vector3 displacement, Vector3 normal)
    {
        float magnitude = displacement.magnitude;
        displacement = Vector3.ProjectOnPlane(displacement, normal).normalized;
        return displacement * magnitude;
    }

    // Source: Collide And Slide - *Actually Decent* Character Collision From Scratch, Improved Collision detection and Response
    // URL: https://www.youtube.com/watch?v=YR6Q7dUz2uk&t=522s, https://www.peroxide.dk/papers/collision/collision.pdf
    // Author: Poke Dev, Kasper Fauerby 
    // Notes: 
    public CollisionInfo ResolveCollideAndSlide(Vector3 displacement, int depth, bool gravityPass, Vector3 velInit,
        ICollisionShape collisionShape, Transform currTransform)
    {
        CollisionInfo outputInfo;
        if (depth >= _maxCollideAndSlideDepth)
        {
            outputInfo = new CollisionInfo(displacement, true);
            return outputInfo;
        }

        Vector3 origin = currTransform.position;
        float distance = displacement.magnitude;

        // Early exit for very small displacements
        if (distance <= 0.001f)
        {
            bool CollisionHasHit = depth == 0 ? false : true;
            outputInfo = new CollisionInfo(displacement, CollisionHasHit);
            return outputInfo;
        }

        distance += _skinWidth;
        Vector3 direction = displacement.normalized;
        RaycastHit hit;

        bool hasHit = false;
        hasHit = collisionShape.Cast(currTransform.position, direction, distance, _layer, out hit);

        if (hasHit)
        {
            Vector3 snapToSurface = direction * Mathf.Max(0, hit.distance - _skinWidth);
            Vector3 leftover = displacement - snapToSurface;
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (snapToSurface.magnitude < _skinWidth)
            {
                // If the snap distance is less than the skin width, we can consider it a collision
                // and resolve it by returning zero displacement for the snap part
                snapToSurface = Vector3.zero;
            }

            if (angle < _maxSlopeAngle)
            {
                if (gravityPass)
                {
                    // If this is a gravity pass, we can just snap to the surface
                    outputInfo = new CollisionInfo(snapToSurface, true);
                    return outputInfo;
                    ////return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, hit.normal);
            }
            else // wall or steep slope
            {
                float scale = 1 - Vector3 .Dot(
                    new Vector3(hit.normal.x, 0, hit.normal.z).normalized, 
                    -new Vector3(velInit.x, 0, velInit.z).normalized
                    );
                leftover = ProjectAndScale(leftover, hit.normal) * scale;
            }
            outputInfo = new CollisionInfo(snapToSurface + ResolveCollideAndSlide(leftover, depth + 1, gravityPass, velInit, collisionShape, currTransform).collisionDisplacement, true);
            return outputInfo;
        }

        // If no collision was detected, return the original displacement
        outputInfo = new CollisionInfo(displacement, hasHit);
        return outputInfo;
    }
}
