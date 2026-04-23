using System;
using UnityEngine;

public class SheepEnnemy : GroundEnnemy
{
    [SerializeField] GameObject Shell;
    Rigidbody rbShell;
    SphereCollider colShell;

    public bool shellHere = true;

    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] float rollSpeed = 35f;
    [SerializeField] float rollDuration = 2.5f;

    [SerializeField] bool repositionToAttack = false;

    protected override void Start()
    {
        base.Start();

        rbShell = Shell.GetComponent<Rigidbody>();
        colShell = Shell.GetComponent<SphereCollider>();
        colShell.enabled = false;
        rbShell.isKinematic = true;

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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (shellHere)
        {
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
        else
        {
            if (move != "getShell" && move != "stun")
            {
                EyesSetColorTo(colorNormal);
                canLookAtPlayer = false;
                navMesh.speed = speed.x;
                navMesh.angularSpeed = SpeedRotate.x;
                move = "getShell";
            }

            if (move == "getShell")
            {
                navMesh.destination = Shell.transform.position;
                if (Vector3.Distance(transform.position, Shell.transform.position) < 1.5f)
                {
                    ShellBack();
                }
            }
        }
    }

    protected override void AttackPatern()
    {
        /*if (Vector3.Distance(AttackTrigger.position, CurrentTarget.position) <= 1f && CurrentTarget != null)
    {
        AttackStart(-1);
    }*/
        if (CurrentTarget != null && shellHere)
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

        rbShell.AddForce(Vector3.up * 250);

        int RandomNumber = UnityEngine.Random.Range(100, 200);
        if (UnityEngine.Random.Range(0, 1) == 0) RandomNumber = -RandomNumber;
        rbShell.AddForce(Vector3.right * RandomNumber);

        RandomNumber = UnityEngine.Random.Range(100, 200);
        if (UnityEngine.Random.Range(0, 1) == 0) RandomNumber = -RandomNumber;
        rbShell.AddForce(Vector3.forward * RandomNumber);

        colShell.enabled = true;
        shellHere = false;

        invincible = false;
        showDamageDisplayInvincible = true;
    }

    void ShellBack()
    {
        rbShell.isKinematic = true;
        colShell.enabled = false;

        canLookAtPlayer = true;
        navMesh.speed = speed.y;
        navMesh.angularSpeed = SpeedRotate.y;

        Shell.transform.SetParent(transform, false);
        Shell.transform.localPosition = new Vector3(0, 0.07f, 0);

        shellHere = true;
        move = "chase";
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

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        base.StunEnnemy(stunTime, infiniteStun);
        animator.SetInteger("Attack", 0);
    }
}
