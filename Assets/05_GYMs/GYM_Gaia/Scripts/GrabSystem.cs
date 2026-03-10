using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabSystem : MonoBehaviour
{
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float grabStrength;
    [SerializeField] private float rangeForSwallow;
    [SerializeField] private LayerMask grabLayers;
    [SerializeField] private Vector3 downValue = Vector3.down;
    private GameObject currentGrabbedObject;
    void OnSecondPower(InputValue _input)
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
