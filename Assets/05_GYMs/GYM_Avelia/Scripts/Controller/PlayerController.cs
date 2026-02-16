using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    CharacterController controller;
    [SerializeField]
    float speed = 10f, rotationSpeed = 15f;

    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    private Transform cameraRotation;

    Vector2 direction = Vector2.zero, look = Vector2.zero;

    public static Action OnInteract;
    public Action Attack;

    private float offset = -90f;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

    public bool CanMove = true, CanRotate = true;

    void Start()
    {
        controller = controller == null ? GetComponent<CharacterController>() : controller;
        if (cameraRotation == null)
        {
            cameraRotation = Camera.main.transform.parent;
        }

        if (cameraFollow == null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
    }

    void Update()
    {
        Vector3 forward = cameraRotation.forward;
        Vector3 right = cameraRotation.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * direction.y) + (right * direction.x);

        if (CanMove)
        {
            Vector3 finalMovement = new Vector3(moveDirection.x, -1f, moveDirection.z);
            controller.Move(finalMovement * speed * Time.deltaTime);
        }

        if (CanRotate)
        {
            UpdateLookDirection(moveDirection);
        }
    }

    void OnMove(InputValue _input)
    {
        direction = _input.Get<Vector2>();
    }

    void OnInteraction(InputValue _input)
    {
        OnInteract?.Invoke();
    }
    void OnLook(InputValue _input)
    {
        cameraFollow.OnLook(_input.Get<Vector2>());
    }

 
    void UpdateLookDirection(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.01f) return;

        currentDirection = moveDir.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
