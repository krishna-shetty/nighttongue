using UnityEngine;
using UnityEngine.UI;

public class SweetPresenter : ActiveFlavorPresenterBase
{
    private SweetModel _sweetModel;

    [Header("Overflow UI")]
    [SerializeField] private GameObject _overflowFill;
    private Image _overflowImage;

    [SerializeField] private GameObject _overflowBackground;

    protected override void Awake()
    {
        base.Awake();

        if (_overflowFill)
            _overflowImage = _overflowFill.GetComponent<Image>();
    }

    public override void BindModel(IModel model)
    {
        base.BindModel(model);

        if (model is SweetModel sweet)
        {
            if (_sweetModel != null)
            {
                _sweetModel.OnOverflowChanged -= SetOverflow;
                _sweetModel.OnOverflowStarted -= ActivateOverflow;
                _sweetModel.OnOverflowEnded -= DeactivateOverflow;
            }

            _sweetModel = sweet;

            _sweetModel.OnOverflowChanged += SetOverflow;
            _sweetModel.OnOverflowStarted += ActivateOverflow;
            _sweetModel.OnOverflowEnded += DeactivateOverflow;
        }
    }

    public void SetOverflow(float normalizedValue)
    {
        if (_overflowImage)
            _overflowImage.fillAmount = Mathf.Clamp01(normalizedValue);
    }

    public void ActivateOverflow()
    {
        if (_overflowFill && _overflowBackground)
        {
            _overflowBackground.SetActive(true);
            _overflowFill.SetActive(true);
            _overflowImage.fillAmount = 1f;
        }
    }

    public void DeactivateOverflow()
    {
        if (_overflowFill && _overflowBackground)
        {
            _overflowBackground.SetActive(false);
            _overflowFill.SetActive(false);
            _overflowImage.fillAmount = 0f;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();

        DeactivateOverflow();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_sweetModel != null)
        {
            _sweetModel.OnOverflowChanged -= SetOverflow;
            _sweetModel.OnOverflowStarted -= ActivateOverflow;
            _sweetModel.OnOverflowEnded -= DeactivateOverflow;
        }
    }
}
