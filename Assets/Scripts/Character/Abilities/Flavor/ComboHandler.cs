using UnityEngine;
using System.Collections;

public abstract class ComboHandlerBase
{
    protected FlavorManager FlavorManager;
    protected ComboSO Combo;
    public ComboHandlerBase(FlavorManager flavorManager, ComboSO combo)
    {
        FlavorManager = flavorManager;
        Combo = combo;
    }
    public abstract IEnumerator Execute();
}

public class BitterSweet : ComboHandlerBase
{
    public BitterSweet(FlavorManager flavorManager, BitterSweetComboSO combo) : base(flavorManager, combo) 
    { }

    public override IEnumerator Execute()
    {
        if (Combo is BitterSweetComboSO combo)
        {
            float overflow = combo.TotalOverflow;
        }
        else
        {
            Debug.Log("How did we get here? Epic Minecraft reference");
        }
        yield break;
    }
}

public class SweetBitter : ComboHandlerBase
{
    public SweetBitter(FlavorManager flavorManager, SweetBitterComboSO combo) : base(flavorManager, combo) { }
    public override IEnumerator Execute()
    {
        if (Combo is SweetBitterComboSO combo)
        {
            float requiredMashCount = combo.RequiredMashCount;
            float currentMashCount = 0;

            while (currentMashCount < requiredMashCount)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    currentMashCount++;
                    Debug.Log($"Mash count: {currentMashCount}/{requiredMashCount}");
                }
                yield return null;
            }
            Debug.Log("Combo complete!");

        }
        else
        {
            Debug.Log("You must be wondering how I got here xD");
        }
        yield break;
    }
}

public class BitterSalty : ComboHandlerBase
{
    public BitterSalty(FlavorManager flavorManager, BitterSaltyComboSO combo) : base(flavorManager, combo) { }
    public override IEnumerator Execute()
    {
        throw new System.NotImplementedException();
    }
}
