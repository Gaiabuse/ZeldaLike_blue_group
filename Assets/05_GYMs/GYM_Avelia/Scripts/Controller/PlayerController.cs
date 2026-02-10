using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;

    [SerializeField]
    float speed = 10f;

    Vector2 direction;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 movement = new(direction.x, -1f, direction.y);

        controller.Move(movement * speed * Time.deltaTime);
    }

    void OnMove(InputValue _input)
    {
        direction = _input.Get<Vector2>();
        print(direction);

        if (direction.magnitude == 0) return;

        var angle = Mathf.Atan2(direction.y, direction.x);
        angle = Mathf.Rad2Deg * angle;

        transform.rotation = Quaternion.Euler(0, -angle, 0);
    }

}
