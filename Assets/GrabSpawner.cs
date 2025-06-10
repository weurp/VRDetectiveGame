using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject objectToSpawn;
    public Transform spawnLocation;
    public bool spawnOnGrab = true;
    public bool spawnOnRelease = false;
    public bool destroyOriginal = false;

    [Header("Optional Settings")]
    public float spawnDelay = 0f;
    public bool oneTimeOnly = true;

    private XRGrabInteractable grabInteractable;
    private bool hasSpawned = false;

    void Start()
    {
        // Get the XR Grab Interactable component
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on " + gameObject.name);
            return;
        }

        // Subscribe to grab events
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log(gameObject.name + " was grabbed!");

        if (spawnOnGrab && (!oneTimeOnly || !hasSpawned))
        {
            SpawnObject();
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log(gameObject.name + " was released!");

        if (spawnOnRelease && (!oneTimeOnly || !hasSpawned))
        {
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        if (objectToSpawn == null)
        {
            Debug.LogWarning("No object to spawn assigned!");
            return;
        }

        if (spawnDelay > 0)
        {
            Invoke(nameof(DoSpawn), spawnDelay);
        }
        else
        {
            DoSpawn();
        }
    }

    void DoSpawn()
    {
        Vector3 spawnPos = spawnLocation != null ? spawnLocation.position : transform.position + Vector3.up;
        Quaternion spawnRot = spawnLocation != null ? spawnLocation.rotation : transform.rotation;

        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPos, spawnRot);

        hasSpawned = true;

        Debug.Log("Spawned: " + spawnedObject.name + " at " + spawnPos);

        if (destroyOriginal)
        {
            Destroy(gameObject);
        }
    }
}
    }
}
