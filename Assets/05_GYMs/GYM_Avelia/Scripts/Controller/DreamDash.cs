using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
    float DashDurationSeconds = 0, DashLength = 1, DashCoolDownSeconds = .5f, offset = .2f;



    bool IsDashing = false;

    public void OnDash(InputValue _input)
    {
        if (!controller.CanMove || !_input.isPressed) return;

        controller.currentAnimator.SetTrigger("isDashing");
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        if (IsDashing) yield break;

        Vector3 originalPosition = transform.position;
        Vector3 destinationPosition = originalPosition + controller.transform.forward * DashLength;

        float timer = 0;

        // naive approach that will not work in the future. Rn it is not the priority to do better than that
        if (!IsPlaceLandable(destinationPosition))
        {
            Debug.LogWarning($"{destinationPosition} is not landable, cannot dash there ");
            yield break;
        }

        DashSetUp();

        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;
            var portion = timer / DashDurationSeconds;

            controller.transform.position = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));

            yield return null;
        }

        controller.CanMove = true;
        controller.CanRotate = true;
        characterController.enabled = true;

        yield return new WaitForSeconds(DashCoolDownSeconds);

        dashVFX.SetActive(false);

        IsDashing = false;
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
        controller.CanMove = false;
        controller.CanRotate = false;

        dashVFX.SetActive(true);

        characterController.enabled = false;
    }
}
