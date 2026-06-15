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
using UnityEngine.VFX;

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

    [Header("VFX Settings")]
    [SerializeField] private VisualEffect VFX;
    [Tooltip("How long the script will wait for the Erase/Create VFX to finish playing before restoring movement and finalizing state.")]
    [SerializeField] private float vfxDuration = 1.0f;

    [Header("Ui elements")]
    [SerializeField] private Image buttonPressVisual;
    private bool isTutoActionDone = false;
    private bool isTutoActionDone2 = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;
    private GameObject currentObject;
    private List<ErasedObject> objectsErased = new List<ErasedObject>();
    private bool erasedAllObjects;
    private Coroutine HoldTimeCoroutine;
    public bool startEnemyErased { get; private set; }
    
    private DreamDash dash;
    private bool isProcessingVFX = false; // Prevents input/movement recovery while VFX plays
    
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
            RumbleManager.Instance.StopVibration();
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
        RumbleManager.Instance.TriggerVibration(lowFreq, highFreq);
        yield return new WaitForSeconds(duration);
        RumbleManager.Instance.StopVibration();
        rumbleCoroutine = null;
    }

    private void Start()
    {
        objectsErased = new List<ErasedObject>();
        currentPointsForCreate = maxPointsForCreate; 
        buttonPressVisual.gameObject.SetActive(false);
        
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
        // If we are waiting out a VFX duration, block any new input registrations
        if (isProcessingVFX) return;

        switch (inputValue.isPressed)
        {
            case true:
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
                    RumbleManager.Instance.StopVibration();
                }
            
                if (!erasedAllObjects)
                {
                    if (currentObject != null)
                    {
                        // Fire off the Coroutine to handle VFX delay
                        StartCoroutine(EraseOrCreateRoutine());
                        return; // Return early; the routine handles resetting player movement when done
                    }
                }

                // Only restore movement immediately if no action/VFX was triggered
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
            
            // Wait for the Erased All process and its corresponding VFX duration
            yield return StartCoroutine(ErasedAllObjectsRoutine());
            erasedAllObjects = true;

            TriggerRumble(0.8f, 0.8f, 0.15f);
        }
        else
        {
            HoldTimeCoroutine = null;
        }
    }

    private IEnumerator EraseOrCreateRoutine()
    {
        ErasedObject erasedObject = currentObject.GetComponent<ErasedObject>();
        if (erasedObject == null)
        {
            if (player != null) player.CanMove = true;
            if (dash != null) dash.enabled = true;
            yield break;
        }
        
        if (erasedObject.Erased && currentPointsForCreate >= erasedObject.creationCost)
        {
            isProcessingVFX = true;
            
            VFX.SetBool("isDestroying", false);
            VFX.enabled = true;
            VFX.Play();
            
            MusicManager.Instance.PlayCreate();

            // >>> PAUSE CODE HERE: Wait for the VFX duration to finish before mutating world states
            yield return new WaitForSeconds(vfxDuration);

            if (!isTutoActionDone)
            {
                if (tutoIndicator == null) yield break;
                tutoIndicator.StopBlink();
            }
            currentPointsForCreate -= erasedObject.creationCost;
            erasedObject.Create();
            
            if (!objectsErased.Contains(erasedObject)) objectsErased.Add(erasedObject);
        }
        else if (!erasedObject.Erased && currentPointsForCreate <= maxPointsForCreate)
        {
            isProcessingVFX = true;

            VFX.SetBool("isDestroying", true);
            VFX.enabled = true;
            VFX.Play();
            
            MusicManager.Instance.PlayErase();
            
            // >>> PAUSE CODE HERE: Wait for the VFX duration to complete
            yield return new WaitForSeconds(vfxDuration);

            if (!isTutoActionDone)
            {
                if (tutoIndicator == null) yield break;
                tutoIndicator.StopBlink();
            }
            
            currentPointsForCreate += erasedObject.creationCost;
            erasedObject.Erase();
        
            if (objectsErased.Contains(erasedObject))
                objectsErased.Remove(erasedObject);
        }
    
        UpdateNeutralUI();

        // Clean up and restore player mechanics
        isProcessingVFX = false;
        if (player != null) player.CanMove = true;
        if (dash != null) dash.enabled = true;
    }

    private IEnumerator ErasedAllObjectsRoutine()
    {
        if (objectsErased.Count > 0)
        {
            isProcessingVFX = true;

            VFX.SetBool("isDestroying", true);
            VFX.enabled = true;
            VFX.Play();
            MusicManager.Instance.PlayErase();

            // >>> PAUSE CODE HERE: Wait for the VFX duration to clear
            yield return new WaitForSeconds(vfxDuration);

            List<ErasedObject> objList = new List<ErasedObject>();
            
            RaycastHit hitInfo;
            bool didHit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, 5, LayerMask.GetMask("Ground"));
            ErasedObject erasedObj = didHit ? hitInfo.collider.GetComponentInParent<ErasedObject>() : null;
            
            foreach (var obj in objectsErased)
            {
                if (erasedObj == null || erasedObj.gameObject != obj.gameObject)
                {
                    obj.Erase();
                    currentPointsForCreate += obj.creationCost;
                    objList.Add(obj);
                }
            }

            foreach (var obj in objList)
            {
                objectsErased.Remove(obj);
            }
            UpdateNeutralUI();
        }

        // Clean up and restore player mechanics
        isProcessingVFX = false;
        if (player != null) player.CanMove = true;
        if (dash != null) dash.enabled = true;
        HoldTimeCoroutine = null;
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
        maxPointsForCreate = Mathf.Clamp(maxPointsForCreate+1, 0, 3);
        currentPointsForCreate = Mathf.Clamp(currentPointsForCreate+1, 0, 3);
        TransformIndicator.Instance.DisplayNeutralChargeIcon(currentPointsForCreate);
    }
    
    public void LoosePointForCreate()
    {
        maxPointsForCreate = Mathf.Clamp(maxPointsForCreate-1, 0, 3);
        currentPointsForCreate = Mathf.Clamp(currentPointsForCreate-1, 0, 3);
        TransformIndicator.Instance.DisplayNeutralChargeIcon(currentPointsForCreate);
    }
}