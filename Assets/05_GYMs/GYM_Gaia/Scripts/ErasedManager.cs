using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ErasedManager : MonoBehaviour
{
    [Tooltip("LayerMax of objects we can Erased and create")]
    [SerializeField] private LayerMask ErasedLayerMask;
    [Tooltip("The number of object we can create at the same time.")]
    [SerializeField] private int maxPointsForCreate;
    private ErasedObject currentObject;
    private int currentPointsForCreate;
    private void OnTriggerEnter(Collider other)
    {
        int otherObjectLayerMask = 1 << other.gameObject.layer;
        if ((ErasedLayerMask.value & otherObjectLayerMask) != 0)
        {
            Debug.Log(other.gameObject.name);
            if (currentObject == null)
            {
                currentObject = other.GetComponent<ErasedObject>();
                return;
            }
            if (currentObject.gameObject != other.gameObject)
            {
                currentObject = other.GetComponent<ErasedObject>();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int otherObjectLayerMask = 1 << other.gameObject.layer;
        if ((ErasedLayerMask.value & otherObjectLayerMask) != 0)
        {
            if (other.gameObject == currentObject.gameObject)
            {
                currentObject = null;
            }
        }
       
    }
    

    public void OnSecondPower(InputValue inputValue)
    {
        EraseOrCreate();
    }

    private void EraseOrCreate()
    {
        if (currentObject)
        {
            if (currentObject.Erased&& currentPointsForCreate > 0)
            {
                currentObject.Create();
                currentPointsForCreate--;
            }else if (currentPointsForCreate < maxPointsForCreate)
            {
                currentObject.Erase();
                currentPointsForCreate++;
            }
        }
    }
 
}
