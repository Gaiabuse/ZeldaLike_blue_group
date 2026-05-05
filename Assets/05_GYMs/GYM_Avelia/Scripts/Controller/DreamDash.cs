using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;

public class DreamDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    PlayerController controller;

    [SerializeField]
    CharacterController characterController;

    [SerializeField]
    private GameObject dashVFX;

    [SerializeField]
    LayerMask layerWall, layerGround;

    [Header("DashCharacteristic")]
    [SerializeField]
    AnimationCurve DashProggression;
    [SerializeField]
    float DashDurationSeconds = 0, DashLength = 1, DashCoolDownSeconds = .5f, offset = .2f, tolerance = .5f;

    const float EXTRAPOLATION_FACTOR = .1f;

    bool IsDashing = false;

    public void OnDash(InputValue _input)
    {
        if (!enabled || !controller.CanMove || !_input.isPressed) return;

        if (!enabled) return;
        if (!controller.CanMove || !_input.isPressed) return;

        dashVFX.SetActive(true);
        controller.currentAnimator.SetTrigger("isDashing");
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        if (IsDashing) yield break;

        Vector3 originalPosition = transform.position;
        Vector3 destinationPosition = originalPosition + controller.transform.forward * DashLength;

        // naive approach that will not work in the future. Rn it is not the priority to do better than that
        if (!IsPlaceLandable(destinationPosition))
        {
            Debug.LogWarning($"no place found trying to find a better position", this);

            if (FindNearGround(destinationPosition) is Vector3 platform)
            {
                Debug.Log($"found a better place {platform}");
                destinationPosition = platform;
            }
            else yield break;
        }

        DashSetUp();

        yield return DoDashMovement(originalPosition, destinationPosition);

        yield return UndoDashSetUp();
    }

    IEnumerator DoDashMovement(Vector3 originalPosition, Vector3 destinationPosition)
    {
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
        controller.currentAnimator.SetTrigger("isDashing");
        controller.CanMove = false;
        controller.CanRotate = false;

        characterController.enabled = false;
    }

    IEnumerator UndoDashSetUp()
    {
        controller.CanMove = true;
        controller.CanRotate = true;
        characterController.enabled = true;

        controller.currentAnimator.SetTrigger("isDashing");

        yield return new WaitForSeconds(DashCoolDownSeconds);

        dashVFX.SetActive(false);

        IsDashing = false;
    }

    Vector3? FindNearGround(Vector3 at)
    {
        latestAt = at;

        var aboveAt = at + Vector3.up;

        RaycastHit check1;

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
