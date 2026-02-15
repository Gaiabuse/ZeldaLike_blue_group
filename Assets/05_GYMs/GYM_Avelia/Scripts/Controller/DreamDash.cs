using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DreamDash : MonoBehaviour
{
    [SerializeField]
    PlayerController controller;

    [SerializeField]
    float DashDurationSeconds = 0, DashLength = 1;

    [SerializeField]
    AnimationCurve DashProggression;

    bool IsDashing = false;


    public void OnDash(InputValue _input)
    {
        StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        if (IsDashing) yield break;
        IsDashing = true;
        Vector3 originalPosition = transform.position;
        Vector3 destinationPosition = originalPosition + controller.currentDirection * DashLength;

        float timer = 0;


        while (timer < DashDurationSeconds)
        {
            timer += Time.deltaTime;
            var portion = timer / DashDurationSeconds;
            controller.transform.position = Vector3.Lerp(originalPosition, destinationPosition, DashProggression.Evaluate(portion));
            yield return null;
        }

        IsDashing = false;
    }
}
