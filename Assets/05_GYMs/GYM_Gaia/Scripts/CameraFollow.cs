using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways] // Allows the script to run in Edit Mode
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float sensitivity = 0.5f;
    [SerializeField] private List<IsometricParalaxe> parallaxePivotList;

    private float horizontalRotation = 0f;
    private Vector2 lookInput;
    
    // Tracks the target position frame-by-frame to get a clean movement delta
    private Vector3 lastTargetPosition;

    public void OnLook(Vector2 value)
    {
        lookInput = value;
    }

    private void OnEnable()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            FollowTarget();
        }
        
#if UNITY_EDITOR
        else 
        {
            FollowTarget();
        }
#endif
    }

    private void FollowTarget()
    {
        if (target == null || transform.parent == null) return;
        Vector3 targetMovement3D = target.position - lastTargetPosition;
        
        Quaternion cameraRotationOffset = Quaternion.Euler(0, -40f, 0);
        Vector3 cameraRelativeMovement = cameraRotationOffset * targetMovement3D;
        
        float newParallaxX = cameraRelativeMovement.z; 
        float newParallaxY = -cameraRelativeMovement.x;

        Vector2 parallaxDelta = new Vector2(newParallaxX, newParallaxY);
        
        transform.parent.position = target.position;
        horizontalRotation += lookInput.x * sensitivity;

        if (horizontalRotation != 0)
        {
            transform.parent.rotation = Quaternion.Euler(0, horizontalRotation, 0);
        }
        if (targetMovement3D.sqrMagnitude > 0.0001f)
        {
            foreach (IsometricParalaxe pivot in parallaxePivotList)
            {
                if (pivot != null)
                {
                    pivot.Move(parallaxDelta);
                }
            }
        }
        lastTargetPosition = target.position;
    }
}