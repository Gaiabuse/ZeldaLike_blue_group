using System;
using UnityEngine;

public class SheepEnnemy : GroundEnnemy
{
    [SerializeField] GameObject Shell;
    Rigidbody rbShell;
    SphereCollider col;

    public bool shellHere = true;

    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] float rollSpeed = 35f;
    [SerializeField] float rollDuration = 2.5f;

    protected override void Start()
    {
        base.Start();

        rbShell = Shell.GetComponent<Rigidbody>();
        col = Shell.GetComponent<SphereCollider>();
        col.enabled = false;
        rbShell.isKinematic = true;

        invincible = true;
        showDamageDisplayInvincible = false;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            LoseShell();
        }*/
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (move == "roll")
        {
            navMesh.destination = WhereToGoPos;
            timerGeneral -= Time.deltaTime;

            if (Vector3.Distance(WhereToGoPos, transform.position) < 2)
            {
                WhereToGoPos = transform.position + transform.forward * 4;
                navMesh.destination = WhereToGoPos;
            }
            if (timerGeneral <= 0)
            {
                move = "rollEnd";
                animator.SetInteger("Attack", 3);
                canLookAtPlayer = true;
                navMesh.isStopped = true;
                ToogleMainAttack(-1);

                navMesh.speed = speed.x;
                navMesh.angularSpeed = SpeedRotate.x;
                WhereToGoPos = CurrentTarget.position;
            }
        }
    }

    protected override void AttackPatern()
    {
        /*if (Vector3.Distance(AttackTrigger.position, CurrentTarget.position) <= 1f && CurrentTarget != null)
    {
        AttackStart(-1);
    }*/
        if (CurrentTarget != null)
        {
            float distTarget = Vector3.Distance(AttackTrigger.position, CurrentTarget.position);
            if (distTarget >= DistStartAttack && TargetInFieldOfView)
            {
                AttackStart(1);
                navMesh.isStopped = false;
                navMesh.speed = 0;
            }
            else if (distTarget < DistStartAttack)
            {

            }
        }
    }

    public void LoseShell()
    {
        rbShell.isKinematic = false;
        Shell.transform.SetParent(null, true);
        rbShell.linearVelocity = Vector3.zero;
        rbShell.AddForce(transform.up * 75);
        col.enabled = true;
        shellHere = false;

        invincible = false;
        showDamageDisplayInvincible = true;
    }

    protected override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        if (attackID == 2)
        {
            ToogleMainAttack(1);

            canLookAtPlayer = false;
            move = "roll";
            WhereToGoPos = CurrentTarget.position;
            navMesh.speed = rollSpeed;
            navMesh.acceleration = rollSpeed * 2;
            navMesh.angularSpeed = 0;
            navMesh.isStopped = false;
            timerGeneral = rollDuration;
        }
        if (attackID == 4)
        {
            animator.SetInteger("Attack", 0);
            StunEnnemy(2, false);
        }
    }
}
