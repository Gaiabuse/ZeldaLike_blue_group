     using System;
     using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine.VFX;

public class DreamDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    PlayerController controller;

    [SerializeField]
    CharacterController characterController;

    [SerializeField]
    private GameObject dashVFX;
    private TrailRenderer dashTrail;

    [SerializeField]
    LayerMask layerWall, layerGround;

    [Header("DashCharacteristic")]
    [SerializeField]
    AnimationCurve DashProggression;
    [SerializeField]
    float DashDurationSeconds = 0, DashLength = 1, DashCoolDownSeconds = .5f, offset = .2f, tolerance = .5f;

    const float EXTRAPOLATION_FACTOR = .1f;

    public bool IsDashing = false;

    float bufTimer = 0f;
    bool IsBuffering = false;
    [SerializeField]
    float maxBufferLength = 0.3f;
    
    [SerializeField] private bool isTutoActionDone = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;
    
    private Tween undodashTween;

    private void Start()
    {
        dashTrail = dashVFX.transform.GetChild(0).GetComponent<TrailRenderer>();
    }

    public void Update()
    {
        if (IsBuffering) DoBuffering();
    }

    private void DoBuffering()
    {
        if (!enabled || !controller.CanMove || IsDashing) return;
        IsBuffering = false;

        var bufDur = Time.time - bufTimer;

        if (bufDur <= maxBufferLength)
        {
            StartCoroutine(Dash());
        }
    }

    public void OnDash(InputValue _input)
    {
        if (!_input.isPressed) return;
        if (!enabled || !controller.CanMove || IsDashing)
        {
            IsBuffering = true;
            bufTimer = Time.time;
            return;
        }

        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        Vector3 originalPosition = transform.position;
        Vector3 destinationPosition = originalPosition + controller.transform.forward * DashLength;

        // naive approach that will not work in the future. Rn it is not the priority to do better than that
        if (IsPlaceLandable(destinationPosition))
        {
            latestAt = destinationPosition;
            latestAtHitResult = true;
        }
        else
        {
            Debug.LogWarning($"no place found trying to find a better position", this);

            if (FindNearGround(destinationPosition) is Vector3 platform)
            {
                Debug.Log($"found a better place {platform}");
                destinationPosition = platform;
            }
            else
            {
                latestAt = destinationPosition;
                latestAtHitResult = false;
                yield break;
            }
        }
        
        if (undodashTween != null) undodashTween.Kill();
        
        dashTrail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
        dashVFX.SetActive(true);
        
        controller.currentAnimator.SetTrigger("isDashing");

        DashSetUp();

        yield return DoDashMovement(originalPosition, destinationPosition);

        yield return UndoDashSetUp();
    }

    IEnumerator DoDashMovement(Vector3 originalPosition, Vector3 destinationPosition)
    {
        MusicManager.Instance.PlayDash();
        float timer = 0;

        Vector3 currentLerpPosition = originalPosition;

        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;
            var portion = timer / DashDurationSeconds;
            Vector3 targetPosition = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));
            transform.position = targetPosition;
            currentLerpPosition = targetPosition;

            yield return null;
        }
    }

    bool IsPlaceLandable(Vector3 destination)
    {
        if (IsThereAWall(destination)) return false;

        Ray ray = new(origin: destination, direction: Vector3.down);
        return Physics.Raycast(ray, 2f, layerGround);
    }

    bool IsThereAWall(Vector3 destination)
        => Physics.Linecast(transform.position + Vector3.up * offset, destination + Vector3.up * offset, layerWall);

    void DashSetUp()
    {
        IsDashing = true;
        //controller.currentAnimator.SetTrigger("isDashing");
        controller.CanMove = false;
        controller.CanRotate = false;

        characterController.enabled = false;
    }

    IEnumerator UndoDashSetUp()
    {
        controller.CanMove = true;
        controller.CanRotate = true;
        characterController.enabled = true;

        //controller.currentAnimator.SetTrigger("isDashing");

        yield return new WaitForSeconds(DashCoolDownSeconds);
        
        float startValue = 1f;
        float endValue = 0f;
        
        undodashTween = DOVirtual.Float(startValue, endValue, 0.5f, (float value) =>
        {
            AnimationCurve curve = dashTrail.widthCurve;
            curve.MoveKey(0, new Keyframe(0f, value));
            curve.MoveKey(1, new Keyframe(1f, value));
            dashTrail.widthCurve = curve;
        }).OnComplete(() =>
        {
            dashVFX.SetActive(false);
        });

        IsDashing = false;
        if (!isTutoActionDone)
        {
            if (tutoIndicator == null) yield return null;
            tutoIndicator.StopBlink();
        }
    }

    Vector3? FindNearGround(Vector3 at)
    {
        latestAt = at;

        var aboveAt = at + Vector3.up;

        RaycastHit check1;

        // SphereCast utilisé ici (et pas un simple Raycast) pour donner une tolérance
        // autour du point recherché : ça capte un sol même si le point exact est limite.
        if (!Physics.SphereCast(aboveAt, tolerance, Vector3.down, out check1))
        {
            latestAtHitResult = false;
            return null;
        }

        var extrapolatedLandingPoint = Vector3.LerpUnclamped(aboveAt, check1.point, EXTRAPOLATION_FACTOR);

        if (IsPlaceLandable(extrapolatedLandingPoint))
        {
            latestAtHitResult = true;
            return extrapolatedLandingPoint + Vector3.up;
        }

        latestAtHitResult = false;
        return null;
    }

    Vector3 latestAt = Vector3.zero;
    bool latestAtHitResult = false;

    void OnDrawGizmos()
    {
        Gizmos.color = latestAtHitResult ? Color.green : Color.red;
        Gizmos.DrawWireSphere(latestAt, tolerance);
    }
}
