using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ErasedManager : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerHP playerHP;
    [Tooltip("LayerMax of objects we can Erased and create")]
    [SerializeField] private LayerMask ErasedLayerMask;
    [Tooltip("The number of object we can create at the same time.")]
    [SerializeField] private int maxPointsForCreate;
    [Tooltip("Hold time for erased all objects we have create")]
    [SerializeField] private float holdTime;
    [SerializeField] private float numberOfPressForErasedEnemy = 10;
    [SerializeField] private int hpHealWhenErasedEnemy = 20;
    private float currentPressForErasedEnemy;
    private GameObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
    private int currentPointsForCreate;
    private bool erasedAllObjects;
    private Coroutine HoldTimeCoroutine;
    public bool startEnemyErased{get; private set;}
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
                currentObject = other.gameObject;
                return;
            }
            if (currentObject.gameObject != other.gameObject)
            {
                currentObject = other.gameObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int otherObjectLayerMask = 1 << other.gameObject.layer;
        if ((ErasedLayerMask.value & otherObjectLayerMask) != 0)
        {
            if (currentObject != null)
            {
                if (other.gameObject == currentObject)
                {
                    currentObject = null;
                }
            }
        }
    }
    public void OnSecondPower(InputValue inputValue)
    {
        if(startEnemyErased)
        {
            currentPressForErasedEnemy++;
            if (currentPressForErasedEnemy >= numberOfPressForErasedEnemy)
            {
                ErasedEnemy();
            }
            return;
        }
        if (currentObject.CompareTag("Ennemy"))
        {
            Debug.Log("startEnemyErased");
            player.CanMove = false;
            player.CanRotate = false;
            startEnemyErased = true;
            currentPressForErasedEnemy = 0;
        }
        switch (inputValue.isPressed)
        {
            case true:
            {
                if(HoldTimeCoroutine != null) StopCoroutine(HoldTimeCoroutine);
                HoldTimeCoroutine = StartCoroutine(HoldTime());
                break;
            }
            case false:
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

                break;
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
            ErasedObject erasedObject = currentObject.GetComponent<ErasedObject>();
            if (erasedObject.Erased&& currentPointsForCreate > 0)
            {
                erasedObject.Create();
                currentPointsForCreate--;
                objectsErased.Add(erasedObject);
            }else if (currentPointsForCreate < maxPointsForCreate)
            {
                erasedObject.Erase();
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

    private void ErasedEnemy()
    {
        Destroy(currentObject);
        currentObject = null;
        playerHP.Heal(hpHealWhenErasedEnemy);
        player.CanMove = true;
        player.CanRotate = true;
        startEnemyErased = false;
        currentPressForErasedEnemy = 0;
    }
 
}
