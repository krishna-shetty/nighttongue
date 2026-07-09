using System;
using UnityEngine;

public class ComboFactory
{
    private readonly FlavorManager _flavorManager;

    public ComboFactory(FlavorManager flavorManager)
    {
        _flavorManager = flavorManager;
    }

    public ComboHandlerBase CreateComboHandler(ComboSO combo)
    {
        return combo switch
        {
            BitterSweetComboSO bitterSweetCombo => new BitterSweet(_flavorManager, bitterSweetCombo),
            SweetBitterComboSO sweetBitterCombo => new SweetBitter(_flavorManager, sweetBitterCombo),
            _ => throw new ArgumentException($"Unsupported combo type: {combo.GetType().Name}")
        };
    }
}
