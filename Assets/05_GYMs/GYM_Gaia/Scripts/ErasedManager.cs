using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ErasedManager : MonoBehaviour
{
    [SerializeField] private LayerMask ErasedLayerMask;
    [SerializeField] private ManaGauge manaGauge;

    private ErasedObject currentObject;
    private void OnTriggerStay(Collider other)
    {
        int otherObjectLayerMask = 1 << other.gameObject.layer;
        if ((ErasedLayerMask.value & otherObjectLayerMask) != 0)
        {
            Debug.Log(other.gameObject.name);
            if (currentObject == null)
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

    public void OnDash(InputValue inputValue)
    {
        if (currentObject)
        {
            if (currentObject.Erased == false) return;
            currentObject.Create();
            manaGauge.DecreaseDivision();
        }
    }

    public void OnSecondPower(InputValue inputValue)
    {
        if (currentObject)
        {
            if (currentObject.Erased) return;
            currentObject.Erase();
            manaGauge.IncreaseDivision();
        }
    }
 
}
