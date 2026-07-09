using System;
using UnityEngine;

public class SweetRuntime : ActiveFlavorRuntimeBase, IFlavorRuntime
{
    public override EFlavor FlavorType => EFlavor.Sweet;

    private SweetModel _sweetModel;
    private SweetFlavorSO _sweetData;
    private SweetPresenter _sweetPresenter;

    private TongueController _tongueController;

    public SweetRuntime(
        FlavorManager flavorManager,
        SweetPresenter presenter,
        SweetFlavorSO data
    ) : base(
        flavorManager,
        presenter,
        new SweetModel(data.Battery),
        data
    )
    {
        _sweetPresenter = presenter;
        _sweetData = data;
        _tongueController = flavorManager.GetComponentInChildren<TongueController>();

        Debug.Log("SweetRuntime :: Constructor called!");

        _sweetModel = (SweetModel)_model;

        _sweetModel.OnOverflowStarted += HandleOverflowStarted;
        _sweetModel.OnOverflowEnded += HandleOverflowEnded;
        _sweetModel.OnOverflowChanged += HandleOverflowChanged;
    }

    public override void OnActivate()
    {
        //_tongueController.IsAiming = false;
        base.OnActivate();
        // Sweet-specific startup logic (if needed)
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        HandleOverflowEnded();
        _tongueController.IsAiming = false;
    }

    private void HandleOverflowStarted()
    {
        if (_sweetPresenter) _sweetPresenter.ActivateOverflow();
    }

    private void HandleOverflowEnded()
    {
        if (_sweetPresenter) _sweetPresenter.DeactivateOverflow();
    }

    private void HandleOverflowChanged(float normalized)
    {
        if (_sweetPresenter) _sweetPresenter.SetOverflow(normalized);
    }

    ~SweetRuntime()
    {
        if (_sweetModel != null)
        {
            _sweetModel.OnOverflowStarted -= HandleOverflowStarted;
            _sweetModel.OnOverflowEnded -= HandleOverflowEnded;
            _sweetModel.OnOverflowChanged -= HandleOverflowChanged;
        }
    }
}
