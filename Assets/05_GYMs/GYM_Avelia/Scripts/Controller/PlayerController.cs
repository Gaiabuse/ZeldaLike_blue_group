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
    private Transform cameraRotation;

    Vector2 direction = Vector2.zero, look = Vector2.zero;

    public static Action OnInteract;
    public Action Attack;

    private float offset = -90f;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

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

    void Update()
    {
        Vector3 forward = cameraRotation.forward;
        Vector3 right = cameraRotation.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * direction.y) + (right * direction.x);

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
        if (moveDir.sqrMagnitude < 0.01f) return;

        currentDirection = moveDir.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);

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
