using UnityEngine;

public class SheepEnnemy : GroundEnnemy
{
    [SerializeField] GameObject Shell;
    Rigidbody rb;
    SphereCollider col;

    public bool shellHere = true;

    protected override void Start()
    {
        base.Start();

        rb = Shell.GetComponent<Rigidbody>();
        col = Shell.GetComponent<SphereCollider>();
        col.enabled = false;
        rb.isKinematic = true;

        invincible = true;
        showDamageDisplayInvincible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoseShell();
        }
    }

    public void LoseShell()
    {
        rb.isKinematic = false;
        Shell.transform.SetParent(null, true);
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(transform.up * 75);
        col.enabled = true;
        shellHere = false;

        invincible = false;
        showDamageDisplayInvincible = true;
    }
}
