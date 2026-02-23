using UnityEngine;

public class Player_Test : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    [SerializeField] Transform CammeraPose;
    [SerializeField] float speed;
    [SerializeField] float speedRotate;

    [SerializeField] float jumpPower = 25f;

    [Header("Gravity")]
    [SerializeField] Transform GroundCheckPose;
    [SerializeField] float GroundCheckRadius = 0.1f;
    [SerializeField] bool isGrounded;

    [SerializeField] bool isOnWall;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        isOnWall = false;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (!isOnWall)
        {
            rb.linearVelocity = new Vector3(horizontal * speed, rb.linearVelocity.y, vertical * speed);
        }
        else
        {
            rb.linearVelocity = transform.right * horizontal * speed + transform.forward * vertical * speed;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnWall)
            {
                rb.AddForce(transform.up * jumpPower * 10, ForceMode.Impulse);

                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                isOnWall = false;
                rb.useGravity = true;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
                rb.linearVelocity += transform.up * jumpPower;
            }
        }
    }

    private void FixedUpdate()
    {
        isGrounded = GroundCheck();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            isOnWall = !isOnWall;

            if (isOnWall)
            {
                transform.rotation = Quaternion.Euler(-90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                rb.useGravity = false;

                RaycastHit hit;
                if (Physics.Raycast(GroundCheckPose.position, -transform.up, out hit, 1f))
                {

                }
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                rb.useGravity = true;
            }
        }
    }

    bool GroundCheck()
    {
        Collider[] hitGroundLayer = Physics.OverlapSphere(GroundCheckPose.position, GroundCheckRadius);

        if (hitGroundLayer.Length > 0)
        {
            foreach (Collider col in hitGroundLayer)
            {
                if (col.gameObject.CompareTag("Ground")) return true;
            }
        }

        return false;
    }
}
