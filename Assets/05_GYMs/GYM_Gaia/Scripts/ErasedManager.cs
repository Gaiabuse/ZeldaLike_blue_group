using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ErasedManager : MonoBehaviour
{
    [Tooltip("LayerMax of objects we can Erased and create")]
    [SerializeField] private LayerMask ErasedLayerMask;
    [Tooltip("The number of object we can create at the same time.")]
    [SerializeField] private int maxPointsForCreate;
    [Tooltip("Hold time for erased all objects we have create")]
    [SerializeField] private float holdTime;
    private ErasedObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
    private int currentPointsForCreate;
    private bool erasedAllObjects;
    private Coroutine HoldTimeCoroutine;
    private void Start()
    {
        objectsErased = new List<ErasedObject>();
    }

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
        if (inputValue.isPressed)
        {
            if(HoldTimeCoroutine != null) StopCoroutine(HoldTimeCoroutine);
            HoldTimeCoroutine = StartCoroutine(HoldTime());
        }

        if (!inputValue.isPressed)
        {
            if(HoldTimeCoroutine != null)StopCoroutine(HoldTimeCoroutine);
            if (erasedAllObjects)
            {
                ErasedAllObjects();
                erasedAllObjects = false;
            }
            else
            {
                EraseOrCreate();
            }
            
        }
            
    }

    private IEnumerator HoldTime()
    {
        erasedAllObjects = false;
        yield return new WaitForSeconds(holdTime);
        erasedAllObjects = true;
        HoldTimeCoroutine = null;
    }
    private void EraseOrCreate()
    {
        if (currentObject)
        {
            if (currentObject.Erased&& currentPointsForCreate > 0)
            {
                currentObject.Create();
                currentPointsForCreate--;
                objectsErased.Add(currentObject);
            }else if (currentPointsForCreate < maxPointsForCreate)
            {
                currentObject.Erase();
                currentPointsForCreate++;
            }
        }
    }

    private void ErasedAllObjects()
    {
        if (objectsErased.Count > 0)
        {
            foreach (var obj in objectsErased)
            {
                obj.Erase();
                currentPointsForCreate++;
            }
        }
    }
 
}
