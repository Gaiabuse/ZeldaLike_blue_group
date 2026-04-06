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
            yield break;
        }

        DashSetUp();

        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;

            var portion = timer / DashDurationSeconds;
            var destinationThisFrame = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));
            var motion = destinationThisFrame - transform.position;

            characterController.Move(motion);

            yield return null;
        }

        controller.CanMove = true;
        controller.CanRotate = true;

        yield return new WaitForSeconds(DashCoolDownSeconds);

        dashVFX.SetActive(false);

        IsDashing = false;
    }

    bool IsPlaceLandable(Vector3 destination)
    {

        if (!IsThereAWall(destination)) return false;

        Ray ray = new(origin: destination, direction: Vector3.down);
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

        characterController.excludeLayers = LayerMask.GetMask("everything");
    }
}
