using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class GrabSystem : MonoBehaviour
{
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float grabStrength;
    [SerializeField] private float rangeForSwallow;
    [SerializeField] private LayerMask grabLayers;
    [SerializeField] private Vector3 downValue = Vector3.down;

    [SerializeField] private float throwDistance;
    [SerializeField] private float throwDuration;

    [SerializeField]
    private GameObject throwMark;

    private GameObject currentGrabbedObject;
    private bool CanThrow = true, IsThrowing = false;

    void Start()
    {
        throwMark.transform.localPosition = Vector3.forward * throwDistance;
    }

    void OnSecondPower(InputValue _input)
    {
        if (IsThrowing) return;

        if (!CanThrow) { CanThrow = true; return; }

        if (currentGrabbedObject == null)
        {
            if (!_input.isPressed) return;

            Grab();

            //eat the next input
            CanThrow = false;
            return;
        }

        if (_input.isPressed)
        {
            ShowThrowPrediction();
            return;
        }

        Throw(_input);
    }

    private void Throw(InputValue _input)
    {
        IsThrowing = true;
        currentGrabbedObject.SetActive(true);

        var landingSpot = transform.position + transform.forward * throwDistance;

        var animation = currentGrabbedObject.transform.DOMove(landingSpot, throwDuration);
        animation.onComplete += CleanUpThrow;
        animation.Play();
    }

    private void CleanUpThrow()
    {
        currentGrabbedObject = null;
        IsThrowing = false;
    }

    private void ShowThrowPrediction()
    {
        throwMark.SetActive(true);
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

        if (Physics.Raycast(downPosition, transform.forward, out RaycastHit hitGrabbed, rangeForGrab, grabLayers))
        {
            Debug.Log(hitGrabbed.collider.gameObject.name);
            Vector3 direction = (hitGrabbed.transform.position - transform.position).normalized;

            if (hitGrabbed.collider.transform.parent != null)
            {

                Rigidbody grabbedObject = hitGrabbed.collider.transform.parent.GetComponent<Rigidbody>();
                if (grabbedObject != null)
                {
                    grabbedObject.AddForce(direction * grabStrength, ForceMode.Impulse);
                }
            }
            else
            {
                Rigidbody grabbedObject = hitGrabbed.collider.gameObject.AddComponent<Rigidbody>();
                if (grabbedObject != null)
                {
                    grabbedObject.AddForce(direction * grabStrength, ForceMode.Impulse);
                }
            }

            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.green);
        }
        else
        {
            Debug.DrawRay(downPosition, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position - downValue, transform.forward * rangeForGrab);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - downValue, transform.forward * rangeForSwallow);
    }
}
