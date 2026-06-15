using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class BasicSheep : GroundEnnemy
{
    SphereCollider sheepCollider;

    [SerializeField] float DistStartAttack = 5f;
    [SerializeField] float ChargeDuration = 0.5f;
    [SerializeField] VisualEffect chargeVFX;
    [SerializeField] float rollSpeed = 35f;
    [SerializeField] float rollDuration = 2.5f;
    [SerializeField] float stunRollEndDuration = 2f;

    [SerializeField] float DistanceGetInShell = 2f;

    bool repositionToAttack = false;

    protected override void Start()
    {
        base.Start();
        chargeVFX.SetFloat("TimeBeforeAttack", ChargeDuration);
        sheepCollider = GetComponent<SphereCollider>();

        invincible = false;
        //showDamageDisplayInvincible = false;
        animator.SetInteger("Shell", 1);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (CurrentTarget == null) CurrentTarget = Player;
        float distPlayer = Vector3.Distance(CurrentTarget.position, transform.position);

        if (move == "roll")
        {
            invincible = true;
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
            invincible = false;
            if (Vector3.Distance(WhereToGoPos, transform.position) <= 2.5f)
            {
                WhereToGoPos = transform.position + (CurrentTarget.transform.forward * (DistStartAttack + 5));
                navMesh.destination = WhereToGoPos;
            }
            if (distPlayer + 0.5f >= DistStartAttack && distPlayer > DistanceGetInShell)
            {
                animator.SetBool("Chase", false);
                repositionToAttack = false;
                canLookAtPlayer = true;

                AttackStart(1);
                move = "aim roll";
                navMesh.isStopped = false;
                navMesh.speed = 0;
            }
            /*if (distPlayer <= DistanceGetInShell)
            {
                GetInShell();
            }*/
        }
        if (move == "aim roll")
        {
            invincible = false;
            Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
            Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.5f);
        }
        if (move == "charge")
        {
            invincible = true;
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
            invincible = false;
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
                invincible = true;
            }
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
                /*else if (distTarget <= DistanceGetInShell && move != "shell")
                {
                    GetInShell();
                }*/
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

    /*void GetInShell()
    {
        move = "shell";
        animator.SetInteger("Shell", 2);
        navMesh.isStopped = true;
        repositionToAttack = false;
        invincible = true;
    }*/

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
            chargeVFX.enabled = false;
            chargeVFX.enabled = true;
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
        navMesh.isStopped = true;
    }

    public override void TakeDamage(int damage, float stun)
    {
        // --- IF THE SHEEP IS HIDDEN IN ITS WOOL (INVINCIBLE) ---
        if (invincible)
        {
            // Reset any active UI DOTween animations so they don't overlap
            if (dotween != null)
            {
                dotween.Kill();
                if (hitValueDisplay) hitValueDisplay.transform.localScale = Vector3.zero;
            }
            dotween = null;

            // Show the invincible message
            if (hitValueDisplay)
            {
                hitValueDisplay.text = "0"; // Change this to whatever text you want
                ShowHitDisplay();
            }

            return; // Stop the script here so it doesn't take damage or trigger hit animations
        }

        // --- IF THE SHEEP IS VULNERABLE (NOT INVINCIBLE) ---
        base.TakeDamage(damage, stun);
    
        if (hitVFX)
        {
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
            hitVFX.transform.LookAt(lookTarget);
            hitVFX.transform.Rotate(0, 90, 0);
            hitVFX.SetActive(true);
        }

        animator.SetBool("IsChasing", false);
        animator.SetBool("IsMoving", false);
        animator.SetTrigger("tHit");

        navMesh.isStopped = false;
        if (stun > 0)
        {
            navMesh.velocity = Vector3.zero;
        }
        else
        {
            animator.SetBool("IsMoving", true);
        }

        if (HP > 0)
        {
            if (move != "stun")
            {
                move = "chase";
                animator.SetBool("IsChasing", true);

                EyesSetColorTo(colorChase);

                navMesh.speed = speed.y;
                navMesh.acceleration = acceleration.y;
                navMesh.angularSpeed = SpeedRotate.y;

                canLookAtPlayer = true;
                WhereToGoPos = Player.position;
            }
        }
    }
}
