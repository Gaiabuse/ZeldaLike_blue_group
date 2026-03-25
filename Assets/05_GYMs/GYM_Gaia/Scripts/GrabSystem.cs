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
    [SerializeField] private float grabStrength;
    [SerializeField] private float rangeForSwallow;
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
            print($"{_input.isPressed} grab");
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

        if (Physics.Raycast(downPosition, transform.forward, out RaycastHit hitSwallow, rangeForSwallow, grabLayers))
        {
            PutGrabMarkAtTarget(hitSwallow.collider.transform.position);
            return;
        }

        if (Physics.Raycast(downPosition, transform.forward, out RaycastHit hitGrabbed, rangeForGrab, grabLayers))
        {
            PutGrabMarkAtTarget(hitGrabbed.collider.transform.position);
            return;
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

        if (Physics.Raycast(downPosition, transform.forward, out RaycastHit hitSwallow, rangeForSwallow, grabLayers))
        {
            currentGrabbedObject = hitSwallow.collider.gameObject;
            if (currentGrabbedObject != null && currentGrabbedObject.transform.parent != null)
            {
                currentGrabbedObject = currentGrabbedObject.transform.parent.gameObject;
            }
            currentGrabbedObject.SetActive(false);
            return;
        }

        RaycastHit hitGrabbed;

        if (!Physics.Raycast(downPosition, transform.forward, out hitGrabbed, rangeForGrab, grabLayers))
        {
            Debug.DrawRay(downPosition, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
            return;
        }

        Debug.Log(hitGrabbed.collider.gameObject.name);
        Vector3 direction = (hitGrabbed.transform.position - transform.position).normalized;

        Rigidbody grabbedObject;

        if (hitGrabbed.collider.transform.parent != null)
        {
            grabbedObject = hitGrabbed.collider.transform.parent.gameObject.AddComponent<Rigidbody>();
        }
        else
        {
            grabbedObject = hitGrabbed.collider.gameObject.AddComponent<Rigidbody>();
        }

        if (grabbedObject != null)
        {
            grabbedObject.AddForce(direction * grabStrength, ForceMode.Impulse);
        }

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.green);
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
}
