using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.AI.Navigation;
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
    public int maxPointsForCreate;
    [SerializeField] private int currentPointsForCreate;
    [Tooltip("Hold time for erased all objects we have create")]
    [SerializeField] private float holdTime;
    [SerializeField] private float numberOfPressForErasedEnemy = 20;
    [SerializeField] private int hpHealWhenErasedEnemy = 20;

    [Header("Ui elements")]
    [SerializeField]private Image buttonPressVisual;
    private float currentPressForErasedEnemy;
    private GameObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
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
        currentPointsForCreate = maxPointsForCreate; 
        buttonPressVisual.gameObject.SetActive(false);
    }
    
    void Update()
    {
        UpdateNeutralUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherObjectLayerMask = 1 << other.gameObject.layer;
        if ((ErasedLayerMask.value & otherObjectLayerMask) != 0)
        {
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
        
        if (inputValue.isPressed && currentObject != null && currentObject.CompareTag("Ennemy"))
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
                erasedAllObjects = false;
                if(HoldTimeCoroutine != null) StopCoroutine(HoldTimeCoroutine);
                HoldTimeCoroutine = StartCoroutine(HoldTime());
                break;

            case false:
                if (HoldTimeCoroutine != null)
                {
                    StopCoroutine(HoldTimeCoroutine);
                    HoldTimeCoroutine = null;

                }
                
                if (!erasedAllObjects)
                {
                    if (currentObject != null)
                    {
                        EraseOrCreate();
                    }
                }
                
                if (Gamepad.current != null)
                {
                    Gamepad.current.SetMotorSpeeds(0f,0f);
                }
                
                break;
        }
    }

    private IEnumerator HoldTime()
    {
        erasedAllObjects = false;
        
        if (objectsErased.Count > 0)
        {
            yield return new WaitForSeconds(0.75f);
            
            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(0.25f,0.25f);
            }
            
            yield return new WaitForSeconds(holdTime-0.75f);
            
            ErasedAllObjects();
            erasedAllObjects = true;

            if (Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(0.8f,0.8f);
            }
            
            yield return new WaitForSeconds(0.25f);
        }
        HoldTimeCoroutine = null;
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f,0f);
        }
    }


    private void EraseOrCreate()
    {
        GarbageBehaviors dust = currentObject.GetComponent<GarbageBehaviors>();
        if (dust != null)
        {
            dust.Erase();
            currentObject = null; 
            return; 
        }
        
        ErasedObject erasedObject = currentObject.GetComponent<ErasedObject>();
        if (erasedObject == null) return;
        
        if (erasedObject.Erased && currentPointsForCreate >= erasedObject.creationCost)
        {
            if (erasedObject.Erased && currentPointsForCreate >= erasedObject.creationCost)
            {
                erasedObject.Create();
                currentPointsForCreate -= erasedObject.creationCost;
                if (!objectsErased.Contains(erasedObject)) objectsErased.Add(erasedObject);
            }
            else if (!erasedObject.Erased && currentPointsForCreate < maxPointsForCreate)
            {
                erasedObject.Erase();
                currentPointsForCreate += erasedObject.creationCost;
                if (objectsErased.Contains(erasedObject)) objectsErased.Remove(erasedObject);
            }
        }
        else if (!erasedObject.Erased && currentPointsForCreate <= maxPointsForCreate)
        {
            erasedObject.Erase();
            currentPointsForCreate += erasedObject.creationCost;
        
            if (objectsErased.Contains(erasedObject))
                objectsErased.Remove(erasedObject);
        }
    
        UpdateNeutralUI();
    }

    private void ErasedAllObjects()
    {
        if (objectsErased.Count > 0)
        {
            foreach (var obj in objectsErased)
            {
                if(obj != null) obj.Erase();
            }
            objectsErased.Clear();
            currentPointsForCreate = maxPointsForCreate;
            UpdateNeutralUI();
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
    
    private void UpdateNeutralUI()
    {
        TransformIndicator.Instance.DisplayNeutralChargeIcon(currentPointsForCreate);

        if (currentObject != null)
        {
            ErasedObject erasedObj = currentObject.GetComponent<ErasedObject>();
            GarbageBehaviors dust = currentObject.GetComponent<GarbageBehaviors>();

            if (erasedObj != null)
            {
                TransformIndicator.Instance.DisplayNeutralIcon(erasedObj.Erased ? 0 : 1);
            }
            else if (dust != null)
            {
                TransformIndicator.Instance.DisplayNeutralIcon(1);
            }
        }
        else
        {
            TransformIndicator.Instance.DisplayNeutralIcon(currentPointsForCreate < maxPointsForCreate ? 1 : 0);
        }
    }

    public void GainPointForCreate()
    {
        maxPointsForCreate++;
        currentPointsForCreate++;
        TransformIndicator.Instance.DisplayNeutralChargeIcon(currentPointsForCreate);
    }
}
