using UnityEngine;

public class freeze : MonoBehaviour
{
    float y;
    float z;
    Quaternion quat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        y = gameObject.transform.position.y;
        z = gameObject.transform.position.z;
        quat = gameObject.transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        gameObject.transform.rotation = quat;
        Vector3 vec = new Vector3(gameObject.transform.position.x, y, z);
    }
}
