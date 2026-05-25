using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class GrabSystem : MonoBehaviour
{
    [Header("Do not change")]
    [SerializeField] PlayerController player;

    [Header("Grab")]
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float sideRangeForGrab = 0.1f;
    [SerializeField] private float grabActionDuration = 0.1f;

    [SerializeField] private float rangeForSwallow;
    [SerializeField] private float sideRangeForSwallow = 0.1f;
    [SerializeField] private LayerMask grabLayers;
    [SerializeField] private Vector3 downValue = Vector3.down;
    [SerializeField] private VisualEffect eatVFX;
    [Tooltip("How long the script will wait for the Swallow/Eat VFX to finish playing.")]
    [SerializeField] private float swallowVfxDuration = 1.5f; 

    [SerializeField] private float offsetGrabbedObject = .05f;

    [Header("Throw")]
    [Tooltip("The enemy will end up at this distance of the enemy")]
    [SerializeField] private float throwDistance = 4f;
    [Tooltip("Duration in seconds")]
    [SerializeField] private float throwDuration = .1f, AutoThrowDuration = 2f;

    [Header("Visual")]
    [SerializeField] private GameObject throwMark, grabMark;
    [SerializeField] private GameObject grabVfx;

    [SerializeField] private GameObject currentGrabbedObject;

    private bool CanThrow = true, IsThrowing = false;
    private bool isGrabbing = false; // Prevents overlapping grab triggers

    enum GrabbingState
    {
        None,
        ShowGrabPred,
        ShowThrowPred,
        TimerLimitThrow,
    }

    private GrabbingState grabbingState;
    private float throwTimer;

    void Start()
    {
        throwMark.transform.localPosition = Vector3.forward * throwDistance;
    }

    void Update()
    {
        DoIndicatorLogic();
        DoStateGrab();
    }

    void DoIndicatorLogic()
    {
        if (currentGrabbedObject != null)
        {
            TransformIndicator.Instance.DisplayNightmareIcon(2);
            return;
        }

        Vector3 downPosition = transform.position - downValue;

        if (DoGrabCheck(downPosition, rangeForSwallow, sideRangeForSwallow) is RaycastHit hit)
        {
            TransformIndicator.Instance.DisplayNightmareIcon(1);
        }
        else
        {
            TransformIndicator.Instance.DisplayNightmareIcon(0);
        }
    }

    void DoStateGrab()
    {
        switch (grabbingState)
        {
            case GrabbingState.None:
                break;
            case GrabbingState.ShowGrabPred:
                ShowGrabPredictionUpdate();
                break;
            case GrabbingState.ShowThrowPred:
                break;
            case GrabbingState.TimerLimitThrow:
                break;
        }
    }

    void OnSecondPower(InputValue _input)
    {
        if (IsThrowing || isGrabbing) return;

        if (currentGrabbedObject == null)
        {
            ProcessGrab(_input);
            return;
        }

        ProcessThrow(_input);
    }

    private void ProcessThrow(InputValue _input)
    {
        if (_input.isPressed)
        {
            ShowThrowPrediction();
            player.CanMove = false;
            player.CanRotate = true;
            return;
        }

        Throw();
        player.CanMove = true;
    }

    private void ProcessGrab(InputValue _input)
    {
        if (_input.isPressed)
        {
            ShowGrabPrediction();
            player.CanMove = false;
            return;
        }

        // Fire off the Coroutine instead of a standard method
        if (!isGrabbing)
        {
            StartCoroutine(GrabRoutine());
        }
    }

    private IEnumerator GrabRoutine()
    {
        isGrabbing = true;
        Vector3 downPosition = transform.position - downValue;

        // --- 1. SWALLOW LOGIC ---
        if (DoGrabCheck(downPosition, rangeForSwallow, sideRangeForSwallow) is RaycastHit hitSwallow)
        {
            currentGrabbedObject = hitSwallow.collider.gameObject;
            if (currentGrabbedObject != null)
            {
                // Activate and play your VFX
                eatVFX.enabled = true;
                eatVFX.Play();

                // PAUSE CODE HERE: Holds script execution right here until the VFX finishes
                yield return new WaitForSeconds(swallowVfxDuration);

                bool isSheep = false;
                SheepEnnemyTest SheepEnnemyScript = currentGrabbedObject.GetComponent<SheepEnnemyTest>();
                if (SheepEnnemyScript != null && SheepEnnemyScript.shellHere)
                {
                    SheepEnnemyScript.LoseShell();
                    isSheep = true;
                }
                
                SheepEnnemySprite SheepSprite = currentGrabbedObject.GetComponent<SheepEnnemySprite>();
                if (SheepSprite != null && SheepSprite.shellHere)
                {
                    SheepSprite.LoseShell();
                    isSheep = true;
                }

                if (currentGrabbedObject.transform.parent != null)
                    currentGrabbedObject = currentGrabbedObject.transform.parent.gameObject;

                if (!isSheep) currentGrabbedObject.SetActive(false);
            }

            // Wrap up state cleanly after VFX concludes
            player.CanMove = true;
            throwTimer = Time.time; 
            isGrabbing = false;
            yield break; 
        }

        // --- 2. STANDARD ATTRACT LOGIC ---
        RaycastHit? maybeHitGrabbed = DoGrabCheck(downPosition, rangeForGrab, sideRangeForGrab);

        if (maybeHitGrabbed is null)
        {
            player.CanMove = true;
            isGrabbing = false;
            yield break;
        }

        RaycastHit hitGrabbed = maybeHitGrabbed ?? throw new Exception("Unreachable");

        GameObject targetSubject = hitGrabbed.transform.parent != null
            ? hitGrabbed.transform.parent.gameObject
            : hitGrabbed.collider.gameObject;

        // PAUSE CODE HERE: Wait for the object to finish flying towards the player
        yield return StartCoroutine(AttractObjectRoutine(targetSubject));

        // Wrap up state cleanly after movement completes
        player.CanMove = true;
        throwTimer = Time.time;
        isGrabbing = false;
    }

    private IEnumerator AttractObjectRoutine(GameObject subject)
    {
        var finalPosition = transform.position + transform.forward * offsetGrabbedObject;

        if (!Physics.Raycast(finalPosition + Vector3.up, Vector3.down * 2f)) yield break;

        var tween = subject.transform.DOMove(finalPosition, grabActionDuration);
        GameObject vfxInstance = null;

        if (grabVfx)
        {
            vfxInstance = Instantiate(grabVfx, subject.transform);
        }

        tween.Play();
        
        // DOTween Coroutine Integration: Pauses here dynamically until the tween completes
        yield return tween.WaitForCompletion();

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }

    private void Throw()
    {
        grabbingState = GrabbingState.None;
        grabMark.SetActive(false);

        IsThrowing = true;
        TransformIndicator.Instance.DisplayNightmareIcon(0);

        Collider collider = currentGrabbedObject.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        currentGrabbedObject.SetActive(true);

        currentGrabbedObject.transform.position = transform.position + Vector3.up * 2f;
        var landingSpot = transform.position + transform.forward * throwDistance;

        EnnemyBase isEnnemy = currentGrabbedObject.GetComponent<EnnemyBase>();
        if (isEnnemy != null) isEnnemy.StunEnnemy(2, false);

        var animation = currentGrabbedObject.transform.DOMove(landingSpot, throwDuration);

        animation.onComplete += () =>
        {
            if (collider != null) collider.enabled = true;
            CleanUpThrow();
        };

        animation.Play();
    }

    private void CleanUpThrow()
    {
        throwMark.SetActive(false);
        currentGrabbedObject = null;
        IsThrowing = false;
    }

    private void ShowThrowPrediction()
    {
        throwMark.SetActive(true);
        grabbingState = GrabbingState.ShowThrowPred;
    }

    private void ShowGrabPrediction()
    {
        grabbingState = GrabbingState.ShowGrabPred;
    }

    private void ShowGrabPredictionUpdate()
    {
        grabMark.SetActive(true);

        Vector3 downPosition = transform.position - downValue;

        var grabchecks = DoGrabCheck(downPosition, rangeForSwallow, sideRangeForSwallow) ?? DoGrabCheck(downPosition, rangeForGrab, sideRangeForGrab);

        if (grabchecks is RaycastHit hit)
        {
            PutGrabMarkAtTarget(hit.collider.transform.position);
            return;
        }

        grabMark.SetActive(false);
    }

    private void PutGrabMarkAtTarget(Vector3 position)
    {
        grabMark.transform.position = position;
    }

    private void DoAutoThrowUpdate()
    {
        var PassedTime = Time.time - throwTimer;
        if (PassedTime < AutoThrowDuration) return;
        Throw();
        grabbingState = GrabbingState.None;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position - downValue, transform.forward * rangeForGrab);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - downValue, transform.forward * rangeForSwallow);
    }

    private void OnDisable()
    {
        IsThrowing = false;
        throwMark.SetActive(false);
        player.CanMove = true;
        grabMark.SetActive(false);
    }

    private RaycastHit? DoGrabCheck(Vector3 down, float range, float siderange)
    {
        if (Physics.SphereCast(down, siderange, transform.forward, out RaycastHit hitGrabbed, range, grabLayers))
        {
            Debug.DrawRay(transform.position, transform.forward * range, Color.green);
            return hitGrabbed;
        }

        Debug.DrawRay(transform.position, transform.forward * range, Color.red);

        return null;
    }
}