using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [SerializeField]private PlayerController playerController;
    [SerializeField]private CharacterController characterController;
    private Vector3 surfaceNormal = Vector3.up;
    

    void Update()
    {
        RaycastHit hit;
        Vector3 rayDir = (transform.forward + -transform.up).normalized;

        if (Physics.Raycast(transform.position, rayDir, out hit, rayDistance, wallLayer))
        {
            characterController.slopeLimit = 90f;
            playerController.surfaceNormal = hit.normal;
            playerController.CanRotate = false;
        }
        else
        {
            playerController.surfaceNormal = Vector3.up;
            playerController.CanRotate = true;
        }
        
    }

  
}