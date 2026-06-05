using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class BasicSheep : GroundEnnemy
{
    SphereCollider sheepCollider;

    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] float ChargeDuration = 0.5f;
    [SerializeField] float rollSpeed = 35f;
    [SerializeField] float rollDuration = 2.5f;
    [SerializeField] float stunRollEndDuration = 2f;

    [SerializeField] float DistanceGetInShell = 2f;

    bool repositionToAttack = false;

    protected override void Start()
    {
        base.Start();
        sheepCollider = GetComponent<SphereCollider>();

        invincible = false;
        showDamageDisplayInvincible = false;
        animator.SetInteger("Shell", 1);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        float distPlayer = Vector3.Distance(CurrentTarget.position, transform.position);

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
                invincible = false;
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
            if (CurrentTarget != null)
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

    void GetInShell()
    {
        move = "shell";
        animator.SetInteger("Shell", 2);
        navMesh.isStopped = true;
        repositionToAttack = false;
        invincible = true;
    }

    public void SetShell(int shell)
    {
        animator.SetInteger("Shell", shell);
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
    }

    protected override void Death()
    {
        deathVFX.SetActive(true);
        if (EnnemyManager.Instance != null)
        {
            Debug.Log("remove");
            EnnemyManager.Instance.enemies.Remove(this);
            EnnemyManager.Instance.Check();
        }
        OnDeath?.Invoke(this);
        animator.SetBool("Death", true);
        move = "death";
    }

    public override void TakeDamage(int damage, float stun)
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
