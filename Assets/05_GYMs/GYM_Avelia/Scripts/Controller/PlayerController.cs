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
    float DeadZone = 0f;

    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    public Transform cameraRotation;

    Vector2 direction = Vector2.zero, look = Vector2.zero;

    public static Action OnInteract;
    public Action Attack;

    private float offset = -90f;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

    public Vector3 surfaceNormal;
    public bool CanMove = true, CanRotate = true;

    enum AccelerationState
    {
        Stopped,
        Accelerate,
        CruiseSpeed,
        Decelerate,
    }

    [SerializeField]
    private AccelerationState accelerationState = AccelerationState.Stopped;

    [SerializeField]
    private float MaxSpeed;

    [SerializeField]
    private AnimationCurve accelerationCurve, decelerationCurve;

    [SerializeField]
    private float accelerationDuration, decelerationDuration;
    private float celerityTimer;

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
        if (CanRotate)
        {
            UpdateLookDirection(moveDirection);
        }

        if (CanMove)
        {
            controller.Move(transform.forward * GetSpeed(accelerationState) * Time.deltaTime);
        }
    }

    void OnMove(InputValue _input)
    {
        direction = _input.Get<Vector2>();

        if (direction.magnitude < DeadZone)
        {
            accelerationState = AccelerationState.Decelerate;
            celerityTimer = Time.time;
            return;
        }

        if (accelerationState == AccelerationState.CruiseSpeed ||
                accelerationState == AccelerationState.Accelerate)
        {
            return;
        }

        celerityTimer = Time.time;
        accelerationState = AccelerationState.Accelerate;
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

    private float GetSpeed(AccelerationState state)
        => state switch
        {
            AccelerationState.Stopped => 0f,
            AccelerationState.CruiseSpeed => MaxSpeed,
            AccelerationState.Accelerate => GetAcceleratingSpeed(),
            AccelerationState.Decelerate => GetDeceleratingSpeed(),
            var dir => throw new NotImplementedException($"{nameof(AccelerationState)} {dir} is not supported"),
        };

    private float GetAcceleratingSpeed()
    {
        var progress = (Time.time - celerityTimer) / accelerationDuration;

        if (progress > 1f)
        {
            accelerationState = AccelerationState.CruiseSpeed;
            return MaxSpeed;
        }

        return MaxSpeed * accelerationCurve.Evaluate(progress);
    }

    private float GetDeceleratingSpeed()
    {
        var progress = (Time.time - celerityTimer) / decelerationDuration;

        if (progress > 1f)
        {
            accelerationState = AccelerationState.Stopped;
            return MaxSpeed;
        }

        return MaxSpeed * decelerationCurve.Evaluate(1 - progress);
    }
}
