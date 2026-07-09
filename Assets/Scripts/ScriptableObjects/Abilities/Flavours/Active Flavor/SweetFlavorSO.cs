using UnityEngine;

[CreateAssetMenu(fileName = "SweetFlavorSO", menuName = "Flavor/Active Flavor/Sweet")]
public class SweetFlavorSO : ActiveFlavorSO
{
    public override EFlavor Flavor => EFlavor.Sweet;
}
