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
    private bool isPressing = false;
    
    public bool ContainsLayer(LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) > 0);
    }

    private void Start()
    {
        isPressing = false;
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if(isPressing)return;
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            isPressing = true;
            objectOnPressurePlate = other.gameObject;
            onPressure.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if(!isPressing)return;
        if (objectOnPressurePlate != null)
        {
            if (!objectOnPressurePlate.activeInHierarchy)
            {
                isPressing = false;
                onUnpressure.Invoke();
                objectOnPressurePlate = null;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(isPressing)return;
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            if(!other.gameObject.activeInHierarchy)return;
            isPressing = true;
            objectOnPressurePlate = other.gameObject;
            onPressure.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            if(other.gameObject != objectOnPressurePlate)return;
            isPressing = false;
            onUnpressure.Invoke();
        }
    }
}
