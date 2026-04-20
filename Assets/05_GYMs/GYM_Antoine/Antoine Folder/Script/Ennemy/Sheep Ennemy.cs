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

    [SerializeField] bool repositionToAttack = false;

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
            timerGeneral -= Time.deltaTime;

            if (Vector3.Distance(navMesh.destination, transform.position) <= 2)
            {
                WhereToGoPos = transform.position + transform.forward * 5;
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
        if (repositionToAttack)
        {
            if (Vector3.Distance(CurrentTarget.position, transform.position) > DistStartAttack)
            {
                repositionToAttack = false;
                canLookAtPlayer = true;

                AttackStart(1);
                move = "aim roll";
                navMesh.isStopped = false;
                navMesh.speed = 0;
            }
        }
        if (move == "aim roll")
        {
            Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
            Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.5f);
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
            if (distTarget >= DistStartAttack && TargetInFieldOfView && !repositionToAttack)
            {
                AttackStart(1);
                move = "aim roll";
                navMesh.isStopped = false;
                navMesh.speed = 0;
            }
            else if (distTarget < DistStartAttack && !repositionToAttack)
            {
                repositionToAttack = true;
                canLookAtPlayer = false;
                WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                navMesh.destination = WhereToGoPos;
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
            navMesh.destination = WhereToGoPos;

            navMesh.speed = rollSpeed;
            navMesh.acceleration = rollSpeed * 5;
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
