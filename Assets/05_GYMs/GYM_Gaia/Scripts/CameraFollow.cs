using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField]private float sensitivity = 0.5f;
    
    private float horizontalRotation = 0f;
    private Vector2 lookInput;
    
    public void OnLook(Vector3 value)
    {
        //lookInput = value;
    }
    void LateUpdate()
    {
        transform.parent.position = target.position;
        
        horizontalRotation += lookInput.x * sensitivity ;

        if (horizontalRotation != 0)
        {
            transform.parent.rotation = Quaternion.Euler(0, horizontalRotation, 0);
        }
    }
}
