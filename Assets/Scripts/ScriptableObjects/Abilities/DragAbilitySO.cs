using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Abilities/Suction Throw")]
public class DragAbilitySO : AbilitySO
{
    public float DragRange = 6f;

    public float extendTime = 1f;

    public float retractTime = 1f;

    public float SpringConstantMin = 250f;
    public float SpringConstantMax = 500f;

    public float Damper = 5f;

    public float MinConnectDistance = 1.0f;
    public float DragStretchDistance = 0.2f;
}
