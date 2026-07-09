using System.Collections.Generic;
using UnityEngine;

public class TutorialClipChange : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController baseController;
    [SerializeField] private AnimationClip placeholderClip;
    [SerializeField] private AnimationClip overrideClip;

    void Awake()
    {
        var overrideController = new AnimatorOverrideController(baseController);
        var clipMapping = new KeyValuePair<AnimationClip, AnimationClip>(placeholderClip, overrideClip);
        var list = new List<KeyValuePair<AnimationClip, AnimationClip>> { clipMapping };
        overrideController.ApplyOverrides(list);
        animator.runtimeAnimatorController = overrideController;
    }
}
