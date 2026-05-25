using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private UnityEvent onPressure;
    [SerializeField] private UnityEvent onUnpressure;
    
    private GameObject objectOnPressurePlate;
    private bool isPressing = false;
    
    public bool ContainsLayer(LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) > 0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isPressing) return;
        
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            Debug.Log("Pressed");
            isPressing = true;
            objectOnPressurePlate = other.gameObject;
            onPressure.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPressing) return;
        
        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            if (!other.gameObject.activeInHierarchy) return;
            
            Debug.Log("Stay Pressed");
            isPressing = true;
            objectOnPressurePlate = other.gameObject;
            onPressure.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (!isPressing) return;
        
        if (objectOnPressurePlate == null || !objectOnPressurePlate.activeInHierarchy)
        {
            ReleasePlate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isPressing) return;

        if (ContainsLayer(layerMask, other.gameObject.layer))
        {
            if (other.gameObject == objectOnPressurePlate)
            {
                ReleasePlate();
            }
        }
    }
    
    private void ReleasePlate()
    {
        Debug.Log("Unpressed");
        isPressing = false;
        objectOnPressurePlate = null;
        onUnpressure.Invoke();
    }
}