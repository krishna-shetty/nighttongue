using System.Collections.Generic;
using UnityEngine;

public class WindBlockingDraggableObject : DraggableObject
{
    private GameObject blockedWind;

    void Update()
    {
        if(blockedWind != null)
        {
            (float, float) objectSize = MeshSize.GetSize(gameObject);
            (float, float) windSize = MeshSize.GetSize(blockedWind);
            if ((transform.position.x - objectSize.Item1 / 2) <= (blockedWind.transform.position.x - windSize.Item1 / 2) && (transform.position.x + objectSize.Item1 / 2) >= (blockedWind.transform.position.x + windSize.Item1 / 2))
            {
                WindVolume currWind = blockedWind.GetComponent<WindVolume>();
                currWind.enabled = false;
                currWind.isActive = false;
                currWind.ParticleSystemDisable();
            }
            else
            {
                WindVolume currWind = blockedWind.GetComponent<WindVolume>();
                currWind.enabled = true;
                currWind.isActive = true;
                currWind.ParticleSystemEnable();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<WindVolume>(out WindVolume blockingObject))
        {
            this.blockedWind = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<WindVolume>(out WindVolume blockingObject))
        {
            this.blockedWind = null;
        }
    }
}
