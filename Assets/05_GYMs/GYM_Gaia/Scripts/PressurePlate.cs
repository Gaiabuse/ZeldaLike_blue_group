using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PressurePlate : MonoBehaviour
{
    [SerializeField]private LayerMask layerMask; // Set this in the Inspector

    [SerializeField] private UnityEvent onPressure;
    [SerializeField] private UnityEvent onUnpressure;
    private GameObject objectOnPressurePlate;
    public bool ContainsLayer(LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) > 0);
    }
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            Debug.Log("OnTriggerEnter");
            objectOnPressurePlate = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            if(other.gameObject != objectOnPressurePlate)return;
            Debug.Log("OnTriggerExit");
        }
    }
}
