using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DreamDash : MonoBehaviour
{
    [SerializeField]
    PlayerController controller;

    [SerializeField]
    CharacterController characterController;

    [SerializeField]
    TagHandle tagWall, tagGround;

    [SerializeField]
    float DashDurationSeconds = 0, DashLength = 1, DashCoolDownSeconds = 0.5f;

    [SerializeField]
    AnimationCurve DashProggression;

    [SerializeField] 
    private GameObject dashVFX; 

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

        IsDashing = true;
        controller.CanMove = false;
        controller.CanRotate = false;

        Vector3 originalPosition = transform.position;
        Vector3 destinationPosition = originalPosition + controller.transform.forward * DashLength;

        float timer = 0;
        
        dashVFX.SetActive(true);

        if (IsPlaceLandable(destinationPosition))
        {
            characterController.excludeLayers = LayerMask.GetMask("everything");
        }


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
        var result_cast = Physics.RaycastAll(ray);

        foreach (var hit in result_cast)
        {
            if (hit.transform.CompareTag(tagGround)) return true;

        }

        throw null;
    }

    bool IsThereAWall(Vector3 destination)
    {
        var result_cast = Physics.RaycastAll(transform.position, destination);

        foreach (var hit in result_cast)
        {
            if (hit.transform.CompareTag(tagWall)) return true;

        }

        return false;
    }
}
