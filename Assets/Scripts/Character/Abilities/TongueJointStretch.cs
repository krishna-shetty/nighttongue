using UnityEngine;

public class TongueJointStretch : MonoBehaviour
{
    [Header("Stretch Settings")]
    public float stretchSpeed = 5f;       // How fast it stretches
    public float returnSpeed = 5f;        // How fast it returns
    public float maxLength = 5f;          // Maximum stretch length

    public enum StretchAxis { X, Y, Z }
    [Header("Choose which axis to scale when stretching")]
    public StretchAxis stretchAxis = StretchAxis.X;
    public TongueController controller;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private float currentMaxLength; // dynamic stretch limit
    private bool isColliding = false;
    private bool wasAttached = false;
    private Vector3 frozenScale;
    void Start()
    {
        if (controller == null) controller = GetComponentInParent<TongueController>();

        originalScale = transform.localScale;
        targetScale = originalScale;
        currentMaxLength = maxLength;
    }

    void LateUpdate()
    {
        bool left = Input.GetMouseButton(0);
        bool right = Input.GetMouseButton(1);
        bool attached = controller != null && controller.IsAttached;
        bool aiming = controller != null && controller.IsAimingActive;


        if (right && aiming || left && aiming)
        {
            targetScale = originalScale;
            switch (stretchAxis)
            {
                case StretchAxis.X: targetScale.x = originalScale.x + currentMaxLength; break;
                case StretchAxis.Y: targetScale.y = originalScale.y + currentMaxLength; break;
                case StretchAxis.Z: targetScale.z = originalScale.z + currentMaxLength; break;
            }
        }
        else
        {
            targetScale = originalScale;
            currentMaxLength = maxLength;
            isColliding = false;
        }
        bool stretchingNow = (right || left) && !attached;
        float speed = stretchingNow ? stretchSpeed : returnSpeed;

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetScale,
            speed * Time.deltaTime * maxLength
        );
    }

    public void LimitStretchAtCollision(float distance)
    {
        currentMaxLength = Mathf.Min(distance, maxLength);
        isColliding = true;
        targetScale = transform.localScale; // freeze at collision scale
    }
}
