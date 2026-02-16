using UnityEngine;

public class Player_Test : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] float speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector3(horizontal * speed, rb.linearVelocity.y, vertical * speed);
    }
}
