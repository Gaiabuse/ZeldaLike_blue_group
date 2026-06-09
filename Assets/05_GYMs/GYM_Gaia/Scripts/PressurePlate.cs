using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private UnityEvent onPressure;
    [SerializeField] private UnityEvent onUnpressure;
    
    private HashSet<GameObject> objectsOnPlate = new HashSet<GameObject>();
    private bool isPressing = false;
    
    public bool ContainsLayer(LayerMask mask, int layer)
    {
        return ((mask.value & (1 << layer)) > 0);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!ContainsLayer(layerMask, other.gameObject.layer)) return;

        objectsOnPlate.Add(other.gameObject);

        if (!isPressing)
        {
            isPressing = true;
            onPressure.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!ContainsLayer(layerMask, other.gameObject.layer)) return;
        if (!other.gameObject.activeInHierarchy) return;

        objectsOnPlate.Add(other.gameObject);

        if (!isPressing)
        {
            isPressing = true;
            onPressure.Invoke();
        }
    }

    private void FixedUpdate()
    {
        if (!isPressing) return;

        objectsOnPlate.RemoveWhere(go => go == null || !go.activeInHierarchy);

        if (objectsOnPlate.Count == 0)
        {
            ReleasePlate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ContainsLayer(layerMask, other.gameObject.layer)) return;

        objectsOnPlate.Remove(other.gameObject);

        if (isPressing && objectsOnPlate.Count == 0)
        {
            ReleasePlate();
        }
    }
    
    private void ReleasePlate()
    {
        isPressing = false;
        objectsOnPlate.Clear();
        onUnpressure.Invoke();
    }
}