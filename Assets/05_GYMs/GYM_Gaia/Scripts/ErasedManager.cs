using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    [SerializeField] private float numberOfPressForErasedEnemy = 20;
    [SerializeField] private int hpHealWhenErasedEnemy = 20;

    [Header("Ui elements")]
    [SerializeField]private Image buttonPressVisual;
    private float currentPressForErasedEnemy;
    private GameObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
    private int currentPointsForCreate;
    private bool erasedAllObjects;
    private Coroutine HoldTimeCoroutine;
    public bool startEnemyErased{get; private set;}

    private void OnEnable()
    {
        playerHP.OnTakeDamage += CancelErasedEnemy;
    }

    private void OnDisable()
    {
        playerHP.OnTakeDamage -= CancelErasedEnemy;
    }

    private void Start()
    {
        objectsErased = new List<ErasedObject>();
        buttonPressVisual.gameObject.SetActive(false);
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

    private void BounceUiVisual()
    {
        buttonPressVisual.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.1f).SetEase(Ease.InBounce).OnComplete((
            () =>
            {
                buttonPressVisual.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBounce);
            } ));
    }
    public void OnSecondPower(InputValue inputValue)
    {
        if(startEnemyErased)
        {
            if (inputValue.isPressed)
            {
                currentPressForErasedEnemy++;
                BounceUiVisual();
                if (currentPressForErasedEnemy >= numberOfPressForErasedEnemy)
                {
                
                    ErasedEnemy();
                }
                return;
            }
           
        }
        if(currentObject == null)return;
        if (currentObject.CompareTag("Ennemy"))
        {
            Debug.Log("startEnemyErased");
            buttonPressVisual.gameObject.SetActive(true);
            player.CanMove = false;
            player.CanRotate = false;
            startEnemyErased = true;
            currentPressForErasedEnemy = 0;
            return;
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
            player.currentAnimator.SetTrigger("usingAbility");
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
    public void OnDash(InputValue _input)
    {
        if (startEnemyErased)
        {
            CancelErasedEnemy();
        }
    }

    private void ErasedEnemy()
    {
        Destroy(currentObject);
        currentObject = null;
        buttonPressVisual.gameObject.SetActive(false);
        playerHP.Heal(hpHealWhenErasedEnemy);
        player.CanMove = true;
        player.CanRotate = true;
        startEnemyErased = false;
        currentPressForErasedEnemy = 0;
    }

    private void CancelErasedEnemy()
    {
        buttonPressVisual.gameObject.SetActive(false);
        player.CanMove = true;
        player.CanRotate = true;
        startEnemyErased = false;
        currentPressForErasedEnemy = 0;
    }
 
}
