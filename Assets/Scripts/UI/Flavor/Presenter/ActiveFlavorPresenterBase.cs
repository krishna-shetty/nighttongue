using UnityEngine;
using UnityEngine.UI;

public class ActiveFlavorPresenterBase : MonoBehaviour, IPresenter
{
    [SerializeField] private GameObject _icon;
    [SerializeField] private GameObject _batteryFill;
    [SerializeField] private GameObject _batteryBackground;
    private ActiveModelBase _model;

    protected Image _batteryImage;

    protected virtual void Awake()
    {
        if (_batteryFill)
            _batteryImage = _batteryFill.GetComponent<Image>();
    }

    public virtual void BindModel(IModel model)
    {
        if(model is ActiveModelBase activeModel)
        {
            _model = activeModel;
            activeModel.OnBatteryChanged += SetBattery;
        } 
    }

    public virtual void SetBattery(float normalizedValue)
    {
        if (_batteryImage)
            _batteryImage.fillAmount = Mathf.Clamp01(normalizedValue);
    }

    public virtual void Activate()
    {
        if (_icon) _icon.SetActive(true);
        if (_batteryFill && _batteryBackground)
        {
            _batteryBackground.SetActive(true);
            _batteryFill.SetActive(true);
            _batteryImage.fillAmount = 1f;
        }
    }

    public virtual void Deactivate()
    {
        if (_icon) _icon.SetActive(false);

        if (_batteryFill && _batteryBackground)
        {
            _batteryBackground.SetActive(false);
            _batteryImage.fillAmount = 0f;
            _batteryFill.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        if(_model != null)
        {
            _model.OnBatteryChanged -= SetBattery;
        }
    }
}
