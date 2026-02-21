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

    public Vector3 surfaceNormal;
    public bool CanMove = true, CanRotate = true;
    public bool OnWall = false;
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

    void FixedUpdate()
    {
        Movement();
        AlignPlayer();
    }
    void AlignPlayer()
    {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
    private void Movement()
    {
        Vector3 camRight = cameraRotation.right;
        
        Vector3 camForward = cameraRotation.forward;
        Vector3 moveDirRight = Vector3.ProjectOnPlane(camRight, transform.up).normalized;
        Vector3 moveDirForward = Vector3.ProjectOnPlane(camForward, transform.up).normalized;
        Vector3 moveDirection = (moveDirForward * direction.y) + (moveDirRight * direction.x);

        if (CanMove)
        {
            Vector3 gravityEffect = -transform.up * 5f;
            Vector3 finalVelocity = (moveDirection * speed) + gravityEffect;
        
            controller.Move(finalVelocity * Time.deltaTime);
        }
        
        if (CanRotate ){
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
        Vector3 projectedDirection = Vector3.ProjectOnPlane(moveDir, transform.up);
        if (projectedDirection.sqrMagnitude < 0.01f) return;

        currentDirection = projectedDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(projectedDirection, transform.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
