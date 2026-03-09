using UnityEngine;
using UnityEngine.InputSystem;

public class GrabSystem : MonoBehaviour
{
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float grabStrength;
    [SerializeField] private float rangeForSwallow;
    [SerializeField] private LayerMask grabLayers;
    
    private GameObject currentGrabbedObject;
    void OnSecondPower(InputValue _input)
    {
        if(currentGrabbedObject)return;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitSwallow, rangeForGrab, grabLayers))
        {
            currentGrabbedObject = hitSwallow.collider.gameObject;
            currentGrabbedObject.SetActive(false);
            return;
        }
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitGrabbed, rangeForGrab,grabLayers))
        {
            Debug.Log(hitGrabbed.collider.gameObject.name);
            Vector3 direction = (hitGrabbed.transform.position - transform.position).normalized;
            hitGrabbed.rigidbody.AddForce(direction * grabStrength, ForceMode.Impulse);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.green);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.red);
        }
       
    }
}
