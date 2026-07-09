using UnityEngine;

public interface IPresenter
{
    void Activate();
    void Deactivate();
    void BindModel(IModel model);
}
