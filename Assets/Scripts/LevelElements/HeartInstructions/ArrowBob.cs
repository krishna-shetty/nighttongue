using UnityEngine;

public class ArrowBob : MonoBehaviour
{
    public float BobHeight = 2f;
    public float BobSpeed = 2f;

    void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * BobSpeed) * BobHeight;
        Vector3 finalPos = transform.position + new Vector3(0, bobOffset, 0);

        transform.position = Vector3.Lerp(
            transform.position,
            finalPos,
            Time.deltaTime * 10f
        );
    }
}
