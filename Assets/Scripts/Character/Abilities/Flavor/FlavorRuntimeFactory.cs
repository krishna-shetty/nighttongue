using UnityEngine;

public class FlavorRuntimeFactory
{
    private readonly FlavorManager _flavorManager;
    private readonly UIContextProvider _contextProvider;
    private readonly IFlavorUIContext _uiContext;

    public FlavorRuntimeFactory(FlavorManager flavorManager, UIContextProvider contextProvider)
    {
        _flavorManager = flavorManager;
        _contextProvider = contextProvider;
        _uiContext = _contextProvider?.GetContext<IFlavorUIContext>();
    }

    public IFlavorRuntime CreateRuntime(FlavorSO flavorSO)
    {
        EFlavor flavor = flavorSO.Flavor;
        switch(flavor)
        {
            case EFlavor.Sweet:
                if(flavorSO is SweetFlavorSO sweet)
                {
                    return new SweetRuntime(_flavorManager, _uiContext?.GetFlavorPresenter<SweetPresenter>(flavor), sweet);
                }
                break;
            case EFlavor.Salty:
                if(flavorSO is SaltyFlavorSO salty)
                {
                    return new SaltyRuntime(_flavorManager, null, salty);
                }
                break;
            case EFlavor.Bitter:
                if(flavorSO is BitterFlavorSO bitter)
                {
                    return new BitterRuntime(_flavorManager, null, bitter);
                }
                break;
        }
        return null;
    }
}
