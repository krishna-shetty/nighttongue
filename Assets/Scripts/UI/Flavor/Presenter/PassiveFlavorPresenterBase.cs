using UnityEngine;

public class PassiveFlavorPresenterBase : MonoBehaviour, IPresenter
{
    private PassiveModelBase _model;
    public virtual void BindModel(IModel model)
    {
        if(model is PassiveModelBase passiveModel)
        {
            _model = passiveModel;
            _model.OnDurationCompleted += Deactivate;
        }
        else
        {
            Debug.LogError($"Model of type {model.GetType().Name} is not compatible with PassiveFlavorPresenterBase.");
        }
    }

    public virtual void Activate()
    {
        // Implement activation logic if needed
    }

    public virtual void Deactivate()
    {
        // Implement deactivation logic if needed
    }
}
