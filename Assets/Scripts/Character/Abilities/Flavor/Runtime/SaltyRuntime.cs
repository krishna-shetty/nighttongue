using UnityEngine;

public class SaltyRuntime : ActiveFlavorRuntimeBase
{
    public override EFlavor FlavorType => EFlavor.Salty;

    public SaltyRuntime(
        FlavorManager flavorManager,
        SaltyPresenter presenter,
        SaltyFlavorSO data
    ) : base(
        flavorManager,
        presenter,
        new SaltyModel(data.Battery),
        data
    )
    {
        Debug.Log("SaltyRuntime :: Constructor called!");
    }
}
