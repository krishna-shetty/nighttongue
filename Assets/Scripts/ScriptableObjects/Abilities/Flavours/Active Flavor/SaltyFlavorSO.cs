using UnityEngine;

[CreateAssetMenu(fileName = "SaltyFlavorSO", menuName = "Flavor/Active Flavor/Salty")]
public class SaltyFlavorSO : ActiveFlavorSO
{
    public override EFlavor Flavor => EFlavor.Salty;
}
