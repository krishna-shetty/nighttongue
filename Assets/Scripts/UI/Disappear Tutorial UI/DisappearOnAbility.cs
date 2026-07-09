using UnityEngine;

public class DisappearOnAbility : MonoBehaviour
{
    [Header("Drag")]
    public bool UseDragAttach = false;

    [Header("Grapple")]
    public bool UseGrappleStart = false;

    [Header("Swing")]
    public bool UseSwingStart = false;
    public bool UseSwingForwardBackward = false;
    public bool UseSwingAscendDescend = false;
    public bool UseSwingExit = false;

    private PlayerController _controller;
    private bool _inRange = false;

    void OnEnable()
    {
        _controller = FindFirstObjectByType<PlayerController>();

        if (UseDragAttach) _controller.OnTongueDragAttach += DisableObject;

        if (UseGrappleStart) _controller.OnGrappleStart += DisableObject;

        if (UseSwingStart) _controller.OnSwingStart += DisableObject;

        if (UseSwingForwardBackward)
        {
            _controller.OnSwingForward += DisableObject;
            _controller.OnSwingBackward += DisableObject;
        }

        if (UseSwingAscendDescend)
        {
            _controller.OnSwingAscend += DisableObject;
            _controller.OnSwingDescend += DisableObject;
        }

        if (UseSwingExit)
        {
            _controller.OnSwingEnd += DisableObject;
        }
    }

    private void OnDisable()
    {
        if (UseDragAttach) _controller.OnTongueDragAttach -= DisableObject;

        if (UseGrappleStart) _controller.OnGrappleStart -= DisableObject;

        if (UseSwingStart) _controller.OnSwingStart -= DisableObject;

        if (UseSwingForwardBackward)
        {
            _controller.OnSwingForward -= DisableObject;
            _controller.OnSwingBackward -= DisableObject;
        }

        if (UseSwingAscendDescend)
        {
            _controller.OnSwingAscend -= DisableObject;
            _controller.OnSwingDescend -= DisableObject;
        }

        if(UseSwingExit)
        {
            _controller.OnSwingEnd -= DisableObject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _inRange = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _inRange = false;
    }

    private void DisableObject()
    {
        if (_inRange)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
