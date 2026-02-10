using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    CharacterController controller;

    [SerializeField]
    float speed = 10f;

    Vector2 direction;
    public static Action OnInteract;

    void Start()
    {
        controller = controller is null ? GetComponent<CharacterController>() : controller;
    }

    void Update()
    {
        Vector3 movement = new(direction.x, -1f, direction.y);

        controller.Move(movement * speed * Time.deltaTime);
    }

    void OnMove(InputValue _input)
    {
        direction = _input.Get<Vector2>();
    }

    void OnInteraction(InputValue _input)
    {
        OnInteract?.Invoke();
    }
    void OnLook(InputValue _input)
    {
        var look = _input.Get<Vector2>();

        if (look.magnitude == 0) return;

        var angle = Mathf.Atan2(direction.y, direction.x);
        angle = Mathf.Rad2Deg * angle;

        controller.transform.rotation = Quaternion.Euler(0, -angle, 0);
    }

}
