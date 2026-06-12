using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class GrabSystem : MonoBehaviour
{
    [Header("Do not change")]
    [SerializeField] PlayerController player;
    [SerializeField] private Animator animator;

    [Header("Grab")]
    [SerializeField] private GameObject grabAimVfx;
    [SerializeField] private float rangeForGrab = 3f;
    [SerializeField] private float radiusForGrab = 0.4f;          // <-- renamed & visible
    [SerializeField] private float grabActionDuration = 0.1f;

    [SerializeField] private float rangeForSwallow = 1.5f;
    [SerializeField] private float radiusForSwallow = 0.5f;       // <-- renamed & visible
    [SerializeField] private LayerMask grabLayers;
    [SerializeField] private Vector3 downValue = Vector3.down;
    [SerializeField] private VisualEffect eatVFX;
    [Tooltip("How long the script will wait for the Swallow/Eat VFX to finish playing.")]
    [SerializeField] private float swallowVfxDuration = 1.5f;
    [SerializeField] private float offsetGrabbedObject = .05f;

    [Header("Throw")]
    [Tooltip("The enemy will end up at this distance from the player")]
    [SerializeField] private float throwDistance = 4f;
    [Tooltip("Duration in seconds")]
    [SerializeField] private float throwDuration = .1f, AutoThrowDuration = 2f;

    [Header("Visual")]
    [SerializeField] private GameObject throwMark, grabMark;
    [SerializeField] private GameObject grabVfx;
    [SerializeField] private bool isTutoActionDone = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;

    [SerializeField] private GameObject currentGrabbedObject;

    private bool CanThrow = true, IsThrowing = false;
    private bool isGrabbing = false;

    enum GrabbingState { None, ShowGrabPred, ShowThrowPred, TimerLimitThrow }
    private GrabbingState grabbingState;
    private float throwTimer;

    void Start()
    {
        throwMark.transform.localPosition = Vector3.forward * throwDistance;
        rangeForGrab-=radiusForGrab;
        radiusForSwallow-=radiusForSwallow;
    }

    private void OnValidate()
    {
        rangeForGrab-=radiusForGrab;
        radiusForSwallow-=radiusForSwallow;
    }

    void Update()
    {
        DoIndicatorLogic();
        DoStateGrab();
    }

    void DoIndicatorLogic()
    {
        if (currentGrabbedObject != null)
        {
            TransformIndicator.Instance.DisplayNightmareIcon(2);
            return;
        }

        Vector3 downPosition = transform.position - downValue;

        if (DoGrabCheck(downPosition, rangeForSwallow, radiusForSwallow) is RaycastHit hit)
        {
            TransformIndicator.Instance.DisplayNightmareIcon(1);
        }
        else
        {
            TransformIndicator.Instance.DisplayNightmareIcon(0);
        }
    }

    void DoStateGrab()
    {
        switch (grabbingState)
        {
            case GrabbingState.None: break;
            case GrabbingState.ShowGrabPred: ShowGrabPredictionUpdate(); break;
            case GrabbingState.ShowThrowPred: break;
            case GrabbingState.TimerLimitThrow: break;
        }
    }

    void OnSecondPower(InputValue _input)
    {
        if (!_input.isPressed)
        {
            grabAimVfx.SetActive(false);
        }
        if (IsThrowing || isGrabbing) return;
        if (currentGrabbedObject == null) { ProcessGrab(_input); return; }
        ProcessThrow(_input);
    }

    private void ProcessThrow(InputValue _input)
    {
        if (_input.isPressed) { ShowThrowPrediction(); player.CanMove = false; player.CanRotate = true; return; }
        Throw();
        player.CanMove = true;
    }

    private void ProcessGrab(InputValue _input)
    {
        if (_input.isPressed) { ShowGrabPrediction(); player.CanMove = false; return; }
        if (!isGrabbing) StartCoroutine(GrabRoutine());
    }

    private IEnumerator GrabRoutine()
    {
        animator.SetTrigger("usingAbility");
        if (!isTutoActionDone)
        {
            if (tutoIndicator == null) yield break;
            tutoIndicator.StopBlink();
        }
        isGrabbing = true;
        Vector3 downPosition = transform.position - downValue;

        // --- 1. SWALLOW LOGIC ---
        if (DoGrabCheck(downPosition, rangeForSwallow, radiusForSwallow) is RaycastHit hitSwallow)
        {
            currentGrabbedObject = hitSwallow.collider.gameObject;
            EnnemyBase currentEnnemyGrab = currentGrabbedObject.GetComponent<EnnemyBase>();
            bool dontGrab = currentEnnemyGrab && currentEnnemyGrab.CheckHP() <= 0;

            if (currentGrabbedObject != null && !dontGrab)
            {
                animator.SetBool("GrabSheep", true);
                animator.SetTrigger("usingAbility");
                eatVFX.enabled = true;
                eatVFX.Play();
                yield return new WaitForSeconds(swallowVfxDuration);

                bool isSheep = false;
                SheepEnnemyTest sheepScript = currentGrabbedObject.GetComponent<SheepEnnemyTest>();
                if (sheepScript != null && sheepScript.shellHere) { sheepScript.LoseShell(); isSheep = true; }

                SheepEnnemySprite sheepSprite = currentGrabbedObject.GetComponent<SheepEnnemySprite>();
                if (sheepSprite != null && sheepSprite.shellHere) { sheepSprite.LoseShell(); isSheep = true; }

                if (currentGrabbedObject.transform.parent != null)
                    currentGrabbedObject = currentGrabbedObject.transform.parent.gameObject;

                if (!isSheep) currentGrabbedObject.SetActive(false);
            }

            player.CanMove = true;
            throwTimer = Time.time;
            isGrabbing = false;
            animator.SetBool("GrabSheep", false);
            yield break;
        }

        // --- 2. STANDARD ATTRACT LOGIC ---
        if (DoGrabCheck(downPosition, rangeForGrab, radiusForGrab) is not RaycastHit hitGrabbed)
        {
            player.CanMove = true;
            isGrabbing = false;
            yield break;
        }

        GameObject targetSubject = hitGrabbed.transform.parent != null
            ? hitGrabbed.transform.parent.gameObject
            : hitGrabbed.collider.gameObject;

        yield return StartCoroutine(AttractObjectRoutine(targetSubject));

        player.CanMove = true;
        throwTimer = Time.time;
        isGrabbing = false;
    }

    private IEnumerator AttractObjectRoutine(GameObject subject)
    {
        var finalPosition = transform.position + transform.forward * offsetGrabbedObject;
        if (!Physics.Raycast(finalPosition + Vector3.up, Vector3.down * 2f)) yield break;

        var tween = subject.transform.DOMove(finalPosition, grabActionDuration);
        GameObject vfxInstance = grabVfx ? Instantiate(grabVfx, subject.transform) : null;

        tween.Play();
        yield return tween.WaitForCompletion();

        if (vfxInstance != null) Destroy(vfxInstance);
    }

    private void Throw()
    {
        grabbingState = GrabbingState.None;
        grabMark.SetActive(false);
        IsThrowing = true;
        TransformIndicator.Instance.DisplayNightmareIcon(0);

        Collider collider = currentGrabbedObject.GetComponent<BoxCollider>();
        if (collider != null) collider.enabled = false;

        currentGrabbedObject.SetActive(true);
        currentGrabbedObject.transform.position = transform.position + Vector3.up * 2f;

        var landingSpot = transform.position + transform.forward * throwDistance;
        EnnemyBase isEnnemy = currentGrabbedObject.GetComponent<EnnemyBase>();
        if (isEnnemy != null) isEnnemy.StunEnnemy(2, false);

        var animation = currentGrabbedObject.transform.DOMove(landingSpot, throwDuration);
        animation.onComplete += () => { if (collider != null) collider.enabled = true; CleanUpThrow(); };
        animation.Play();
    }

    private void CleanUpThrow() { throwMark.SetActive(false); currentGrabbedObject = null; IsThrowing = false; }
    private void ShowThrowPrediction() { throwMark.SetActive(true); grabbingState = GrabbingState.ShowThrowPred; }

    private void ShowGrabPrediction()
    {
        grabAimVfx.SetActive(false);
        grabAimVfx.SetActive(true);
        grabbingState = GrabbingState.ShowGrabPred;
    }

    private void ShowGrabPredictionUpdate()
    {
        grabMark.SetActive(true);
        Vector3 downPosition = transform.position - downValue;
        var hit = DoGrabCheck(downPosition, rangeForSwallow, radiusForSwallow)
               ?? DoGrabCheck(downPosition, rangeForGrab, radiusForGrab);

        if (hit is RaycastHit h) { PutGrabMarkAtTarget(h.collider.transform.position); return; }
        grabMark.SetActive(false);
    }

    private void PutGrabMarkAtTarget(Vector3 position) { grabMark.transform.position = position; }

    private void DoAutoThrowUpdate()
    {
        if (Time.time - throwTimer < AutoThrowDuration) return;
        Throw();
        grabbingState = GrabbingState.None;
    }

    /// <summary>
    /// Casts a sphere along the player's forward axis and returns the CLOSEST valid hit.
    /// Using SphereCastAll + manual closest-pick solves the ambiguity when multiple
    /// objects fall inside the enlarged radius — the nearest one always wins.
    /// </summary>
    private RaycastHit? DoGrabCheck(Vector3 origin, float range, float radius)
    {
        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, transform.forward, range, grabLayers);

        if (hits.Length == 0)
        {
            Debug.DrawRay(origin, transform.forward * range, Color.red);
            grabAimVfx.GetComponent<VisualEffect>().SetFloat("Lenght", rangeForGrab);
            return null;
        }

        // Pick the closest hit — deterministic, no random "whoever Unity finds first" surprise
        RaycastHit closest = hits[0];
        for (int i = 1; i < hits.Length; i++)
            if (hits[i].distance < closest.distance)
                closest = hits[i];
        
        float dist = Vector3.Distance(transform.position, closest.transform.position);
        grabAimVfx.GetComponent<VisualEffect>().SetFloat("Lenght", dist);

        Debug.DrawRay(origin, transform.forward * range, Color.green);
        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position - downValue;

        // Swallow zone — red, shorter & fatter
        DrawSphereCastGizmo(origin, transform.forward, rangeForSwallow, radiusForSwallow, Color.red);

        // Grab zone — green, longer & thinner
        DrawSphereCastGizmo(origin, transform.forward, rangeForGrab, radiusForGrab, Color.green);
    }

    /// <summary>
    /// Draws a SphereCast as two end-cap wireframe spheres + connecting lines,
    /// accurately representing what Physics.SphereCastAll will sweep through.
    /// </summary>
    private void DrawSphereCastGizmo(Vector3 origin, Vector3 direction, float range, float radius, Color color)
    {
        Gizmos.color = color;
        direction = direction.normalized;

        Vector3 endPoint = origin + direction * range;

        // Start and end spheres
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(endPoint, radius);

        // Four connecting lines along the "tube" edges (up/down/left/right of travel axis)
        Vector3 perpUp    = Vector3.Cross(direction, Vector3.right).normalized * radius;
        Vector3 perpRight = Vector3.Cross(direction, Vector3.up).normalized    * radius;

        if (perpUp    == Vector3.zero) perpUp    = Vector3.up    * radius;
        if (perpRight == Vector3.zero) perpRight = Vector3.right * radius;

        Gizmos.DrawLine(origin + perpUp,    endPoint + perpUp);
        Gizmos.DrawLine(origin - perpUp,    endPoint - perpUp);
        Gizmos.DrawLine(origin + perpRight, endPoint + perpRight);
        Gizmos.DrawLine(origin - perpRight, endPoint - perpRight);
    }

    private void OnDisable()
    {
        IsThrowing = false;
        throwMark.SetActive(false);
        player.CanMove = true;
        grabMark.SetActive(false);
    }
}