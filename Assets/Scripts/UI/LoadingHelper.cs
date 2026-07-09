using System;
using UnityEngine;

public class LoadingHelper : MonoBehaviour
{
    public static LoadingHelper instance;
    [SerializeField] private Animator anim;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Animator GetAnimator()
    {
        return anim;
    }

}
