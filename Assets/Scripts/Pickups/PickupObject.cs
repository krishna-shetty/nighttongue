using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class PickupObject : MonoBehaviour
{
    [SerializeField]
    private float respawnTime;
    [SerializeField]
    private FlavorSO pickupFlavor;
    private float timer = 0f;
    private bool isActive = true;
    private List<MeshRenderer> childMeshes = new List<MeshRenderer>();
    private bool playerCanPickUp = false;
    private GameObject playerGameObject;

    private TriggerUI triggerUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MeshRenderer[] childMeshesGet = GetComponentsInChildren<MeshRenderer>();
        foreach(var currMesh in childMeshesGet)
        {
            childMeshes.Add(currMesh);
        }
        childMeshes.Remove(GetComponent<MeshRenderer>());

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPickup += HandlePickup;
        }

        triggerUI = GetComponent<TriggerUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            timer += Time.deltaTime;
            if(timer >= respawnTime)
            {
                timer = 0f;
                isActive = true;
                ChangeChildMeshState();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerModel")) return;
        playerGameObject = other.gameObject;
        playerCanPickUp = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerModel")) return;

        playerCanPickUp = false;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnPickup -= HandlePickup;
    }

    private void ChangeChildMeshState()
    {
        foreach(var currMesh in childMeshes)
        {
            currMesh.enabled = !currMesh.enabled;
        }
        if (triggerUI) triggerUI.enabled = !triggerUI.enabled;
    }

    private void HandlePickup()
    {
        if (playerCanPickUp && isActive)
        {
            var flavorManager = playerGameObject.GetComponentInParent<FlavorManager>();

            // if (flavorManager.GetActiveActiveFlavor() is SaltyFlavorSO) return;

            flavorManager.OnFlavorPickup(pickupFlavor);

            ChangeChildMeshState();
            isActive = false;

            //if (pickupFlavor is SaltyFlavorSO) StartCoroutine(ActivateAbilityOnPickup());
        }
    }

    private IEnumerator ActivateAbilityOnPickup()
    {
        yield return null;
        playerGameObject.GetComponentInParent<AbilityUser>().ActivateAbility(pickupFlavor.Abilities[0]);
    }
}
