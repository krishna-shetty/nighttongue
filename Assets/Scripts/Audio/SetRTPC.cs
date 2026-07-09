using UnityEngine;

public class SetRTPC : MonoBehaviour
{
    [SerializeField] private string[] rtpcNames;
    [SerializeField] private Component componentReference;
    [SerializeField] private string valueNameFromComponent;

    public void SetRTPCValueFromComponent(int rtpc)
    {
        if (componentReference == null)
        {
            Debug.LogWarning("SetRTPC: Component reference is null");
            return;
        }

        if (string.IsNullOrEmpty(valueNameFromComponent))
        {
            Debug.LogWarning("SetRTPC: Value name from component is empty");
            return;
        }

        var componentType = componentReference.GetType();
        var fieldInfo = componentType.GetField(valueNameFromComponent);
        var propertyInfo = componentType.GetProperty(valueNameFromComponent);

        float value = 0f;
        if (fieldInfo != null)
        {
            value = (float)fieldInfo.GetValue(componentReference);
        }
        else if (propertyInfo != null)
        {
            value = (float)propertyInfo.GetValue(componentReference);
        }
        else
        {
            Debug.LogWarning($"SetRTPC: Could not find field or property '{valueNameFromComponent}' on component {componentType.Name}");
            return;
        }

        SetRTPCValue(rtpc, value);
    }

    public void SetRTPCValue(int rtpc, float value)
    {
        AkUnitySoundEngine.SetRTPCValue(rtpcNames[rtpc], value);
    }

    public void SetAllRTPCValues(float value)
    {
        foreach (string rtpc in rtpcNames)
        {
            AkUnitySoundEngine.SetRTPCValue(rtpc, value);
        }
    }
}
