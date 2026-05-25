using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.AI.Navigation;
using Unity.VisualScripting;
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
    public int currentPointsForCreate;
    [Tooltip("Hold time for erased all objects we have create")]
    [SerializeField] private float holdTime;

    [Header("Ui elements")]
    [SerializeField] private Image buttonPressVisual;
    private GameObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
    private bool erasedAllObjects;
    private Coroutine HoldTimeCoroutine;
    public bool startEnemyErased { get; private set; }
    
    private DreamDash dash;
    
    public static ErasedManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDisable()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }
    
    private Coroutine rumbleCoroutine;

    private void TriggerRumble(float lowFreq, float highFreq, float duration)
    {
        if (Gamepad.current == null) return;
    
        if (rumbleCoroutine != null)
        {
            StopCoroutine(rumbleCoroutine);
        }
        rumbleCoroutine = StartCoroutine(RumbleRoutine(lowFreq, highFreq, duration));
    }

    private IEnumerator RumbleRoutine(float lowFreq, float highFreq, float duration)
    {
        Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        Gamepad.current.SetMotorSpeeds(0f, 0f);
        rumbleCoroutine = null;
    }

    private void Start()
    {
        objectsErased = new List<ErasedObject>();
        currentPointsForCreate = maxPointsForCreate; 
        buttonPressVisual.gameObject.SetActive(false);
        
        // FIX: Grab the Dash component directly from the assigned Player GameObject to prevent NullReference exceptions
        if (player != null)
        {
            dash = player.GetComponent<DreamDash>();
        }
        else
        {
            Debug.LogError("ErasedManager: PlayerController reference is missing in the Inspector!");
        }
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
    
    public void OnSecondPower(InputValue inputValue)
    {
        switch (inputValue.isPressed)
        {
            case true:
                // Lock movement and dash immediately when action starts
                if (player != null) player.CanMove = false;
                if (dash != null) dash.enabled = false;

                erasedAllObjects = false;
                if (HoldTimeCoroutine != null) StopCoroutine(HoldTimeCoroutine);
                HoldTimeCoroutine = StartCoroutine(HoldTime());
                break;

            case false:
                if (HoldTimeCoroutine != null)
                {
                    StopCoroutine(HoldTimeCoroutine);
                    HoldTimeCoroutine = null;
                }
    
                if (rumbleCoroutine != null)
                {
                    StopCoroutine(rumbleCoroutine);
                    rumbleCoroutine = null;
                }
                if (Gamepad.current != null)
                {
                    Gamepad.current.SetMotorSpeeds(0f, 0f);
                }
            
                if (!erasedAllObjects)
                {
                    if (currentObject != null)
                    {
                        EraseOrCreate();
                    }
                }

                if (player != null) player.CanMove = true;
                if (dash != null) dash.enabled = true;
                break;
        }
    }

    private IEnumerator HoldTime()
    {
        if (objectsErased.Count > 0)
        {
            erasedAllObjects = false;
            yield return new WaitForSeconds(0.20f);
            float remainingHoldTime = Mathf.Max(0f, holdTime - 0.20f);
            TriggerRumble(0.25f, 0.25f, remainingHoldTime); 
    
            yield return new WaitForSeconds(remainingHoldTime);
            
            ErasedAllObjects();
            erasedAllObjects = true;

            TriggerRumble(0.8f, 0.8f, 0.15f);
        }
        
        HoldTimeCoroutine = null;
    }

    private void EraseOrCreate()
    {
        ErasedObject erasedObject = currentObject.GetComponent<ErasedObject>();
        if (erasedObject == null) return;
        
        if (erasedObject.Erased && currentPointsForCreate >= erasedObject.creationCost)
        {
            currentPointsForCreate -= erasedObject.creationCost;
            erasedObject.Create();
            MusicManager.Instance.PlayCreate();
            if (!objectsErased.Contains(erasedObject)) objectsErased.Add(erasedObject);
        }
        else if (!erasedObject.Erased && currentPointsForCreate <= maxPointsForCreate)
        {
            currentPointsForCreate += erasedObject.creationCost;
            erasedObject.Erase();
            MusicManager.Instance.PlayErase();
        
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
                if (obj != null) obj.Erase();
            }
            MusicManager.Instance.PlayErase();
            objectsErased.Clear();
            currentPointsForCreate = maxPointsForCreate;
            UpdateNeutralUI();
        }
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