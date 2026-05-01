using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public Animator currentAnimator;
    [SerializeField]
    CharacterController controller;

    [Header("Player Constants")]
    [SerializeField]
    private Vector3 gravity = new(0f, -10f, 0f);
    [SerializeField]
    float speed = 10f, rotationSpeed = 15f;
    [SerializeField]
    float decayAccel = 5f, decayDecel = 10f;

    private float currentStickProgress, smoothedStickProgress;

    [SerializeField]
    private CameraFollow cameraFollow;

    [SerializeField]
    public Transform cameraRotation;

    [Header("Collision")]
    [SerializeField]
    public LayerMask layerGround;

    [SerializeField]
    float offsetRayCast = .1f, lengthRayCast = .3f, rotationOffset = 0.1f;
    [Header("Anti-Fall Buffer")]
    [SerializeField] float lookAheadDistance = 0.3f; // How far ahead to push the sensor
    [SerializeField] float sensorRadius = 0.4f;      // The width of the ray circle
    [SerializeField] int minRaysRequired = 5;
    [SerializeField] float YLevelDeathPlane = -10f;

    Vector2 direction = Vector2.zero, look = Vector2.zero;

    public Action OnCatch;
    public Action OnRelease;
    public static Action OnRespawn;
    public Action Attack;

    private float offset = -90f;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

    public Vector3 surfaceNormal;
    public bool CanMove = true, CanRotate = true;

    public AttackManager currentAttackManager;
    public MovingBox.Side side = MovingBox.Side.Right;

    [SerializeField] private LayerMask obstacleLayer;
    [HideInInspector] public GameObject Boxes;
    [SerializeField] private bool respawnAtStart = true;

    void Start()
    {
        Boxes = null;
        controller = controller == null ? GetComponent<CharacterController>() : controller;
        if (!PlayerPrefs.HasKey("PlayerSpawnX") && !PlayerPrefs.HasKey("PlayerSpawnY") && !PlayerPrefs.HasKey("PlayerSpawnZ") || respawnAtStart)
        {
            PlayerPrefs.SetFloat("PlayerSpawnX", transform.position.x);
            PlayerPrefs.SetFloat("PlayerSpawnY", transform.position.y);
            PlayerPrefs.SetFloat("PlayerSpawnZ", transform.position.z);
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            StartCoroutine(RespawnCoroutine());
        }

        playerInput = GetComponent<PlayerInput>();
        currentAnimator = currentAttackManager.FormAnimator;
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
        if (transform.position.y < YLevelDeathPlane)
        {
            TriggerRespawn();
            return;
        }

        Vector3 moveDirection = ProjectPoint(direction);

        if (CanRotate) UpdateLookDirection(moveDirection);

        if (!controller.enabled) return;

        if (CanMove)
        {
            var decay = smoothedStickProgress < currentStickProgress ? decayAccel : decayDecel;
            smoothedStickProgress = smoothedStickProgress.expDecay(currentStickProgress, decay, Time.deltaTime);

            var movement = moveDirection * (speed * smoothedStickProgress * Time.deltaTime);
            var futurePosition = transform.position + movement;

            if (IsPlaceLandable(futurePosition)) controller.Move(movement);
            //else Debug.LogWarning("not able to find any ground", this);
        }

        controller.Move(gravity * Time.deltaTime);
    }

    public Vector3 ProjectPoint(Vector2 dir)
    {
        Vector3 camRight = cameraRotation.right;
        Vector3 camForward = cameraRotation.forward;
        Vector3 moveDirRight = Vector3.ProjectOnPlane(camRight, transform.up).normalized;
        Vector3 moveDirForward = Vector3.ProjectOnPlane(camForward, transform.up).normalized;
        return (moveDirForward * dir.y) + (moveDirRight * dir.x);
    }

    void OnMove(InputValue _input)
    {
        var ldirection = _input.Get<Vector2>();
        if (currentAnimator.GetBool("isRunning"))
        {
            currentAnimator.SetFloat("xInput", direction.x);
            currentAnimator.SetFloat("yInput", direction.y);
        }

        currentStickProgress = ldirection.magnitude;

        currentAnimator.SetBool("isRunning", currentStickProgress >= Math.Abs(0.1));
        if (currentStickProgress <= 0.1) return;
        direction = ldirection.normalized;
    }


    void OnLook(InputValue _input)
    {
        cameraFollow.OnLook(_input.Get<Vector2>());
    }

    /*
     void OnRespawn(InputValue _input)
    {
        StartCoroutine(RespawnCoroutine());
    }
    */

    public void TriggerRespawn()
    {

        StartCoroutine(RespawnCoroutine());
    }

    public IEnumerator RespawnCoroutine()
    {
        OnRespawn?.Invoke();
        controller.enabled = false;
        Vector3 startPos = new Vector3(PlayerPrefs.GetFloat("PlayerSpawnX"), PlayerPrefs.GetFloat("PlayerSpawnY"), PlayerPrefs.GetFloat("PlayerSpawnZ"));
        transform.position = startPos;

        controller.enabled = true;
        CanMove = false;
        CanRotate = false;

        yield return new WaitForSeconds(1f);

        CanMove = true;
        CanRotate = true;
    }

    public void Teleport(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }
    void UpdateLookDirection(Vector3 moveDir)
    {
        Vector3 projectedDirection = Vector3.ProjectOnPlane(moveDir, transform.up);
        if (projectedDirection.sqrMagnitude < 0.01f) return;

        currentDirection = projectedDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(projectedDirection, transform.up);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    bool IsPlaceLandable(Vector3 destination)
    {
        // 1. Calculate the offset based on move direction
        // We use currentDirection (normalized) to push the sensor forward
        Vector3 sensorCenter = destination + (currentDirection * lookAheadDistance);

        int rayCount = 8;
        int hitCount = 0;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount) * Mathf.Deg2Rad;

            // 2. Create the circle around the OFFSET center
            float x = Mathf.Cos(angle) * sensorRadius;
            float z = Mathf.Sin(angle) * sensorRadius;

            Vector3 rayOrigin = sensorCenter + new Vector3(x, offsetRayCast, z);

            Ray ray = new Ray(rayOrigin, Vector3.down);
            bool hit = Physics.Raycast(ray, lengthRayCast + offsetRayCast, layerGround);

            // Visual Debugging
            Debug.DrawRay(rayOrigin, Vector3.down * (lengthRayCast + offsetRayCast), hit ? Color.green : Color.red);

            if (hit) hitCount++;
        }

        // 3. Safety: Always ensure the player's actual destination is grounded too
        bool destinationIsSafe = Physics.Raycast(destination + Vector3.up * offsetRayCast, Vector3.down, lengthRayCast + offsetRayCast, layerGround);

        return destinationIsSafe && (hitCount >= minRaysRequired);
    }
}
