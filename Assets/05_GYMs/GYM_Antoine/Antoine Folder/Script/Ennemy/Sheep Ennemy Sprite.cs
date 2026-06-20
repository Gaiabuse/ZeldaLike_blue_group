using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class SheepEnnemySprite : GroundEnnemy
{
    [SerializeField] GameObject Shell;
    Rigidbody rbShell;
    SphereCollider colShell;
    SphereCollider sheepCollider;

    public bool shellHere = true;

    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] float ChargeDuration = 0.5f;
    [SerializeField] float rollSpeed = 35f;
    [SerializeField] float rollDuration = 2.5f;
    [SerializeField] float stunRollEndDuration = 2f;

    [SerializeField] float DistanceGetInShell = 2f;
    [SerializeField] float AbdandonHopeShell = 175f;

    bool repositionToAttack = false;

    protected override void Start()
    {
        base.Start();

        rbShell = Shell.GetComponent<Rigidbody>();
        colShell = Shell.GetComponent<SphereCollider>();
        sheepCollider = GetComponent<SphereCollider>();
        colShell.enabled = false;
        rbShell.isKinematic = true;

        invincible = true;
        showDamageDisplayInvincible = false;
        animator.SetInteger("Shell", 1);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CurrentTarget == null) CurrentTarget = Player;
        float distPlayer = Vector3.Distance(CurrentTarget.position, transform.position);

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
                    move = "roll end";
                    animator.SetInteger("Attack", 3);
                    canLookAtPlayer = true;
                    navMesh.isStopped = true;
                    ToogleMainAttack(-1);
                    sheepCollider.isTrigger = false;
                    timerGeneral = stunRollEndDuration;

                    navMesh.speed = speed.x;
                    navMesh.angularSpeed = SpeedRotate.x;
                    WhereToGoPos = CurrentTarget.position;
                }
            }
            if (move == "reposition")
            {
                if (Vector3.Distance(WhereToGoPos, transform.position) <= 2.5f)
                {
                    WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                    navMesh.destination = WhereToGoPos;
                }
                if (distPlayer > DistStartAttack && distPlayer > DistanceGetInShell)
                {
                    animator.SetBool("Chase", false);
                    repositionToAttack = false;
                    canLookAtPlayer = true;

                    AttackStart(1);
                    move = "aim roll";
                    navMesh.isStopped = false;
                    navMesh.speed = 0;
                }
                if (distPlayer <= DistanceGetInShell)
                {
                    GetInShell();
                }
            }
            if (move == "aim roll")
            {
                Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
                Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.5f);
            }
            if (move == "charge")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    ToogleMainAttack(1);

                    canLookAtPlayer = false;
                    move = "roll";

                    sheepCollider.isTrigger = true;
                    WhereToGoPos = CurrentTarget.position;
                    navMesh.destination = WhereToGoPos;

                    navMesh.speed = rollSpeed;
                    navMesh.acceleration = rollSpeed * 5;
                    navMesh.angularSpeed = 0;
                    navMesh.isStopped = false;

                    timerGeneral = rollDuration;
                }
            }
            if (move == "roll end")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    animator.SetInteger("Attack", 0);
                    navMesh.isStopped = false;
                    PatrolStart();
                }
            }
            if (move == "shell")
            {
                if (distPlayer >= DistanceGetInShell + 1)
                {
                    animator.SetInteger("Shell", 1);
                    WhereToGoPos = CurrentTarget.position;
                    navMesh.destination = WhereToGoPos;
                    navMesh.isStopped = false;
                    move = "patrol";
                }
            }
        }
        else
        {
            if (Shell != null || Vector3.Distance(transform.position, Shell.transform.position) <= AbdandonHopeShell)
            {
                if (move == "shell lost")
                {
                    EyesSetColorTo(colorNormal);
                    canLookAtPlayer = false;
                    navMesh.speed = speed.x;
                    navMesh.angularSpeed = SpeedRotate.x;
                    navMesh.acceleration = acceleration.x;
                    navMesh.isStopped = false;
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
            else
            {
                Death();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(5, 0.5f);
        }
    }

    protected override void AttackPatern()
    {
        if (move != "stun")
        {
            if (CurrentTarget != null && shellHere)
            {
                float distTarget = Vector3.Distance(AttackTrigger.position, CurrentTarget.position);
                if (distTarget >= DistStartAttack && TargetInFieldOfView && !repositionToAttack && move != "shell")
                {
                    AttackStart(1);
                    move = "aim roll";
                    navMesh.isStopped = false;
                    navMesh.speed = 0;
                }
                else if (distTarget < DistStartAttack && !repositionToAttack && distTarget > DistanceGetInShell && move != "shell")
                {
                    repositionToAttack = true;
                    canLookAtPlayer = false;
                    WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                    navMesh.destination = WhereToGoPos;

                    animator.SetBool("Chase", true);
                    navMesh.speed = speed.y;
                    navMesh.angularSpeed = SpeedRotate.y;
                    navMesh.acceleration = acceleration.y;

                    move = "reposition";
                }
                else if (distTarget <= DistanceGetInShell && move != "shell")
                {
                    GetInShell();
                }
            }
        }
    }

    public void LoseShell()
    {
        animator.SetInteger("Shell", -1);
        animator.SetInteger("Attack", 0);
        navMesh.isStopped = true;

        move = "lose shell";
        ToogleMainAttack(-1);

        Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
        Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = lookAtTarget;
    }

    public void SetShell(int shell)
    {
        animator.SetInteger("Shell", shell);
        if (shell == -2)
        {
            Shell.SetActive(true);
            rbShell.isKinematic = false;
            Shell.transform.SetParent(null, true);

            rbShell.linearVelocity = Vector3.zero;
            rbShell.angularVelocity = Vector3.zero;

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
    }

    void ShellBack()
    {
        rbShell.linearVelocity = Vector3.zero;
        rbShell.angularVelocity = Vector3.zero;

        rbShell.isKinematic = true;
        colShell.enabled = false;

        canLookAtPlayer = true;
        navMesh.speed = speed.y;
        navMesh.angularSpeed = SpeedRotate.y;

        Shell.transform.SetParent(transform, false);
        Shell.transform.localPosition = new Vector3(0, 0.07f, 0);
        Shell.SetActive(false);
        animator.SetInteger("Shell", 1);

        shellHere = true;
        PatrolStart();
    }

    void GetInShell()
    {
        move = "shell";
        animator.SetInteger("Shell", 2);
        navMesh.isStopped = true;
        repositionToAttack = false;
    }

    public override void AttackStart(int attackID)
    {
        base.AttackStart(attackID);
        if (attackID == 2)
        {
            timerGeneral = ChargeDuration;
            move = "charge";
        }
    }

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        base.StunEnnemy(stunTime, infiniteStun);
        repositionToAttack = false;
        if (!shellHere)
        {
            animator.SetInteger("Shell", -3);
            move = "shell lost";
        }
    }

    protected override void Death()
    {
        deathVFX.SetActive(true);
        if (EnnemyManager.Instance != null)
        {
            Debug.Log("remove");
            EnnemyManager.Instance.enemies.Remove(this);
        }
        OnDeath?.Invoke(this);
        animator.SetBool("Death", true);
        move = "death";
    }

    public override void TakeDamage(int damage, float stun)
    {
        if (!shellHere)
        {
            base.TakeDamage(damage, stun);
            hitVFX.transform.SetParent(transform.parent);
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
            hitVFX.transform.LookAt(lookTarget);
            hitVFX.transform.Rotate(0, 90, 0);

            hitVFX.SetActive(true);
        }
        else
        {
            if (dotween != null)
            {
                dotween.Kill();
                if (hitValueDisplay) hitValueDisplay.transform.localScale = Vector3.zero;
            }

            dotween = null;
            if (hitValueDisplay)
            {
                hitValueDisplay.text = "Nope";

                ShowHitDisplay();
            }
        }
    }
}
