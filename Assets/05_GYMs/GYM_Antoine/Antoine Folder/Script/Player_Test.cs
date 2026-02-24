using UnityEngine;

public class Player_Test : MonoBehaviour
{
    Rigidbody rb;

    [Header("Movement")]
    [SerializeField] Transform CammeraPose;
    [SerializeField] float speed;
    [SerializeField] float speedRotate;

    [SerializeField] float jumpPower = 25f;

    [Header("Ground Related")]
    [SerializeField] Transform GroundCheckPose;
    [SerializeField] float GroundCheckRadius = 0.1f;
    [SerializeField] bool isGrounded;

    [SerializeField] Transform RayWallCheckPlayer;
    [SerializeField] Transform RayWallCheckPointToWall;
    [SerializeField] Transform WallRayStartPointToWall;

    [SerializeField] bool isOnWall;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        isOnWall = false;

        RayWallCheckPointToWall.SetParent(null, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isOnWall)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(transform.up * jumpPower * 10, ForceMode.Impulse);

                ExitWall();
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

        isGrounded = GroundCheck();
    }

    private void LateUpdate()
    {
        RayWallCheckPointToWall.position = transform.position;
        RayWallCheckPointToWall.rotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            isOnWall = !isOnWall;

            if (isOnWall)
            {
                rb.useGravity = false;

                RaycastHit hit;
                if (Physics.Raycast(RayWallCheckPlayer.position, RayWallCheckPlayer.forward, out hit, 5f))
                {
                    WallRayStartPointToWall.LookAt(hit.point + (other.transform.up * 1));

                    RaycastHit hit2;
                    if (Physics.Raycast(WallRayStartPointToWall.position, WallRayStartPointToWall.forward, out hit2, Mathf.Infinity))
                    {
                        transform.position = hit.point;
                        transform.rotation = WallRayStartPointToWall.rotation;
                    }
                }
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                rb.useGravity = true;
            }
        }
        if (other.CompareTag("WallExit"))
        {
            if (isOnWall)
            {
                ExitWall();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            ExitWall();
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

    void ExitWall()
    {
        rb.linearVelocity = Vector3.zero;

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        isOnWall = false;
        rb.useGravity = true;
    }
}
