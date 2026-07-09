using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinAlphaUI : MonoBehaviour
{
    [Tooltip("The objects with images to set a minimum alpha threshold to detect hits on. Aka images that have empty space you don't want to interfere with pointer events. (Detects images in the object and all its children)")]
    public List<GameObject> Targets;
    public float AlphaThreshold = 0.2f;

    void Start()
    {
        // ugliest code block known to man
        foreach (var t in Targets ?? new())
            foreach (var img in t.GetComponentsInChildren<Image>())
                if (img.mainTexture != null && img.mainTexture.isReadable)
                    img.alphaHitTestMinimumThreshold = AlphaThreshold;
    }
}
