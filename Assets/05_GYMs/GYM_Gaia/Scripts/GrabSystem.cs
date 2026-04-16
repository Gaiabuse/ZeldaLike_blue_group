using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class GrabSystem : MonoBehaviour
{
    [Header("Do not change")]
    [SerializeField]
    PlayerController player;

    [Header("Grab")]
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float sideRangeForGrab = 0.1f;
    [SerializeField] private float grabStrength;

    [SerializeField] private float rangeForSwallow;
    [SerializeField] private float sideRangeForSwallow = 0.1f;
    [SerializeField] private LayerMask grabLayers;
    [SerializeField] private Vector3 downValue = Vector3.down;

    [Header("Throw")]
    [Tooltip("The enemy will end up at this distance of the enemy")]
    [SerializeField] private float throwDistance;
    [Tooltip("Duration in seconds")]
    [SerializeField] private float throwDuration, AutoThrowDuration;

    [Header("Visual")]
    [SerializeField]
    private GameObject throwMark, grabMark;

    private GameObject currentGrabbedObject;

    private bool CanThrow = true, IsThrowing = false;

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
        if (IsThrowing) return;

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

        Grab();
        player.CanMove = true;
        throwTimer = Time.deltaTime;
    }

    private void Throw()
    {
        grabbingState = GrabbingState.None;
        grabMark.SetActive(false);

        IsThrowing = true;
        TransformIndicator.Instance.DisplayNightmareIcon(0);
        currentGrabbedObject.SetActive(true);

        Rigidbody rb = currentGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        currentGrabbedObject.transform.position = transform.position + Vector3.up * 2f;
        var landingSpot = transform.position + transform.forward * throwDistance;

        var animation = currentGrabbedObject.transform.DOMove(landingSpot, throwDuration);

        animation.onComplete += () =>
        {
            if (rb != null) rb.isKinematic = false;
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

        if ((DoGrabCheck(downPosition, rangeForSwallow, sideRangeForSwallow) ??
                    DoGrabCheck(downPosition, rangeForGrab, sideRangeForGrab))
                is RaycastHit hit)
        {
            PutGrabMarkAtTarget(hit.collider.transform.position);
        }

        grabMark.SetActive(false);
    }

    private void PutGrabMarkAtTarget(Vector3 position)
    {
        grabMark.transform.position = position;
    }

    private void Grab()
    {
        Vector3 downPosition = transform.position - downValue;

        // if you don't understand this please check nullable syntax and pattern matching :3 cool stuff
        if (DoGrabCheck(downPosition, rangeForSwallow, sideRangeForSwallow) is RaycastHit hitSwallow)
        {
            currentGrabbedObject = hitSwallow.collider.gameObject;
            if (currentGrabbedObject != null && currentGrabbedObject.transform.parent != null)
            {
                currentGrabbedObject = currentGrabbedObject.transform.parent.gameObject;
            }

            currentGrabbedObject.SetActive(false);
            return;
        }

        RaycastHit? maybeHitGrabbed = DoGrabCheck(downPosition, rangeForGrab, sideRangeForGrab);

        if (maybeHitGrabbed is null) return;

        RaycastHit hitGrabbed = maybeHitGrabbed ?? throw new Exception("Unreachable");

        Vector3 direction = (hitGrabbed.transform.position - transform.position).normalized;

        Rigidbody grabbedObject = GetRigidbodyFromEnemy(hitGrabbed.collider.gameObject);

        if (grabbedObject == null)
        {
            grabbedObject = AddRigidbodyToEnemy(hitGrabbed.collider.gameObject);
        }

        grabbedObject.AddForce(direction * grabStrength, ForceMode.Impulse);

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

    private Rigidbody GetRigidbodyFromEnemy(GameObject enemy)
        => enemy.transform.parent == null ?
             enemy.GetComponent<Rigidbody>() :
             enemy.transform.parent.gameObject.GetComponent<Rigidbody>();

    private Rigidbody AddRigidbodyToEnemy(GameObject enemy)
        => enemy.transform.parent == null ?
             enemy.AddComponent<Rigidbody>() :
             enemy.transform.parent.gameObject.AddComponent<Rigidbody>();

}
