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
    private Vector3 gravity = new(0f, -10f, 0f);

    [SerializeField]
    private float decayAccel = 5f, decayDecel = 10f;
    private float currentStickProgress, smoothedStickProgress;

    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    public Transform cameraRotation;

    Vector2 direction = Vector2.zero, look = Vector2.zero;

    public Action OnCatch;
    public Action OnRelease;
    
    public Action Attack;

    private float offset = -90f;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

    public Vector3 surfaceNormal;
    public bool CanMove = true, CanRotate = true;

    public AttackManager currentAttackManager;
    public MovingBox.Side side = MovingBox.Side.Right;

    public bool IsWithBox = false;
    public bool OnWallWithBox = false;
    void Start()
    {
        IsWithBox = false;
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

        if (IsWithBox)
        {
            switch (side)
            {
                case MovingBox.Side.Left:
                    moveDirection = new Vector3(moveDirection.x, 0, 0);
                    if (OnWallWithBox && moveDirection.x > 0) moveDirection.x = 0; 
                    break;

                case MovingBox.Side.Right:
                    moveDirection = new Vector3(moveDirection.x, 0, 0);
                    if (OnWallWithBox && moveDirection.x < 0) moveDirection.x = 0; 
                    break;

                case MovingBox.Side.Front:
                    moveDirection = new Vector3(0, 0, moveDirection.z);
                    if (OnWallWithBox && moveDirection.z < 0) moveDirection.z = 0; 
                    break;

                case MovingBox.Side.Back:
                    moveDirection = new Vector3(0, 0, moveDirection.z);
                    if (OnWallWithBox && moveDirection.z > 0) moveDirection.z = 0; 
                    break;
            }
        }

        if (CanRotate)
        {
            UpdateLookDirection(moveDirection);
        }

        if (CanMove)
        {
            var decay = smoothedStickProgress < currentStickProgress ? decayAccel : decayDecel;
            smoothedStickProgress = smoothedStickProgress.expDecay(currentStickProgress, decay, Time.deltaTime);

            controller.Move(moveDirection * Time.deltaTime * speed * smoothedStickProgress);
        }

        controller.Move(gravity * Time.deltaTime);

    }
    void OnMove(InputValue _input)
    {
        var ldirection = _input.Get<Vector2>();
        currentStickProgress = ldirection.magnitude;

        if (currentStickProgress <= 0.1) return;

        direction = ldirection.normalized;
    }

    void OnInteraction(InputValue _input)
    {
        //OnInteract?.Invoke();
    }

    void OnCatchOrRelease(InputValue _input)
    {
        if (_input.isPressed)
        {
            Debug.Log("catch");
            OnCatch?.Invoke();
        }
        else
        {
            Debug.Log("release");
            OnRelease.Invoke();
        }
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
