using UnityEngine;

public class FixPosition : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    void Update()
    {
        if (transform.localPosition != originalPosition)
        {
            transform.localPosition = originalPosition;
        }

        if (transform.localRotation != originalRotation)
        {
            transform.localRotation = originalRotation;
        }
        
    }
}
