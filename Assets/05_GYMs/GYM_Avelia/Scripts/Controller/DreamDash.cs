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
                Debug.Log($"found a better place");
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

        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;
            var portion = timer / DashDurationSeconds;

            controller.transform.position = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));

            yield return null;
        }

        yield break;
    }

    bool IsPlaceLandable(Vector3 destination)
    {
        if (IsThereAWall(destination)) return false;

        Ray ray = new(origin: destination + Vector3.up * offset, direction: Vector3.down);
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
        controller.currentAnimator.SetTrigger("isDashing");
        characterController.enabled = true;

        yield return new WaitForSeconds(DashCoolDownSeconds);

        dashVFX.SetActive(false);

        IsDashing = false;
    }

    Vector3? FindNearGround(Vector3 at)
    {
        var platforms = Physics.OverlapSphere(at, tolerance, layerGround);

        // fuck you why are you dashing in the void
        latestAtHitResult = platforms.Length != 0;
        latestAt = at;

        if (platforms.Length == 0) return null;

        var placeToBeReplacedAt = platforms
            .Select(x => x.GetComponent<Collider>())
            .Select(x => Physics.ClosestPoint(at, x, x.transform.position, x.transform.rotation))
            .OrderBy(x => Vector3.Distance(x, at))
            .First();

        var extrapolatedPlace = Vector3.LerpUnclamped(at, placeToBeReplacedAt, 1.1f);

        Ray ray = new(origin: extrapolatedPlace, direction: Vector3.down);
        Physics.Raycast(ray, 2f, layerGround);

        return placeToBeReplacedAt + Vector3.up * 3f;
    }

    Vector3 latestAt = Vector3.zero;
    bool latestAtHitResult = false;

    void OnDrawGizmos()
    {
        Gizmos.color = latestAtHitResult ? Color.green : Color.red;
        Gizmos.DrawWireSphere(latestAt, tolerance);
    }
}
