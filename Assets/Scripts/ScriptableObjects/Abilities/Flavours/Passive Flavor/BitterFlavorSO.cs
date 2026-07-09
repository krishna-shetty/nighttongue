using UnityEngine;

[CreateAssetMenu(fileName = "BitterFlavorSO", menuName = "Flavor/Passive Flavor/Bitter")]
public class BitterFlavorSO : PassiveFlavorSO
{
    public override EFlavor Flavor => EFlavor.Bitter;
}
