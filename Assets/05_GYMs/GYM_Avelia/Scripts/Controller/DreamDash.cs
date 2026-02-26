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
    float DashDurationSeconds = 0, DashLength = 1;

    [SerializeField]
    AnimationCurve DashProggression;

    bool IsDashing = false;


    public void OnDash(InputValue _input)
    {
        if (!controller.CanMove && !_input.isPressed) return;
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


        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;
            var portion = timer / DashDurationSeconds;
            var destinationThisFrame = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));

            var motion = destinationPosition - transform.position;

            characterController.Move(motion);
            yield return null;
        }

        IsDashing = false;
        controller.CanMove = true;
        controller.CanRotate = true;
    }
}
