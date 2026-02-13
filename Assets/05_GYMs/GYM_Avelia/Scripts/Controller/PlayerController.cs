using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    CharacterController controller;

    [SerializeField]
    float speed = 10f;

    Vector2 direction = Vector2.zero;
    Vector2 look = Vector2.zero;

    public Vector3 currentDirection { get; private set; } = Vector3.forward;

    void Start()
    {
        controller = controller == null ? GetComponent<CharacterController>() : controller;
    }

    void Update()
    {
        Vector3 movement = new(direction.x, -1f, direction.y);

        controller.Move(movement * speed * Time.deltaTime);

        updateLookDirection();
    }

    void OnMove(InputValue _input)
    {
        direction = _input.Get<Vector2>();
    }

    void OnLook(InputValue _input)
    {
        look = _input.Get<Vector2>();
    }

    void updateLookDirection()
    {
        var lookDir = look != Vector2.zero ? look : direction;

        if (lookDir == Vector2.zero) return;

        currentDirection = new Vector3(lookDir.x, 0, lookDir.y);

        var angle = Mathf.Atan2(lookDir.y, lookDir.x);
        angle = Mathf.Rad2Deg * angle;

        controller.transform.rotation = Quaternion.Euler(0, -angle, 0);
    }
}
