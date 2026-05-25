using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteAlways] // Allows the script to run in Edit Mode
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float sensitivity = 0.5f;

    private float horizontalRotation = 0f;
    private Vector2 lookInput;
    
    public void OnLook(Vector3 value)
    {
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
        if (target != null && transform.parent != null) 
        {
            transform.parent.position = target.position;
        }

        horizontalRotation += lookInput.x * sensitivity;

        if (horizontalRotation != 0 && transform.parent != null)
        {
            transform.parent.rotation = Quaternion.Euler(0, horizontalRotation, 0);
        }
    }
}