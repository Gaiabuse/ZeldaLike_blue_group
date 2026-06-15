using UnityEngine;
using System.Collections.Generic;

public class BookEnnemy : EnnemyBase
{
    Rigidbody rb;

    [Header("Flying Ennemy")]
    [SerializeField] GameObject SpriteEnnemy;
    [SerializeField] float LookRange = 12f;
    [SerializeField] float DistanceFromGround = 5;
    [SerializeField] float FallWait = 0.25f;
    [SerializeField] float MaxFallTime = 5;
    [SerializeField] float StunTimeRecoverFromAttack = 2;
    float lastY;

    [Header("Melee Setting")]
    [SerializeField] bool canUseMelee = true;
    [SerializeField] float FallWhenDistance = 0.5f;
    [SerializeField] float addForceDive = 3;

    [Header("Range Setting")]
    [SerializeField] Vector2 FireRange = new Vector2(9, 12);
    [SerializeField] bool canUseProjectile = false;
    [SerializeField] GameObject Projectile;
    [SerializeField] Vector3 spawnProjectile;
    [SerializeField] List<float> Cooldown;
    int currentCooldown = 0;
    float cooldownAttack;

    [Header("Layer")]
    [SerializeField] LayerMask LayerTarget;
    [SerializeField] LayerMask GroundLayer;

    protected override void Start()
    {
        base.Start();
        if (MainHitBox != null) MainHitBox.damage = data.strength;
        lastY = transform.position.y;
        move = "0";

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearDamping = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (Cooldown.Count > 0) cooldownAttack = Cooldown[currentCooldown];
        else cooldownAttack = 0;

        animator.SetBool("IsMoving", true);
        animator.SetBool("IsChasing", false);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        isPlayerInFieldOfView();

        if (move != "stun")
        {
            float DistancePlayer = Vector3.Distance(transform.position, CurrentTarget.position);
            float DistancePlayerDown = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(CurrentTarget.position.x, CurrentTarget.position.z));

            if (move == "wait" || move == "0" || move == "targetInRange")
            {
                RaycastHit GroundDistHit;
                if (Physics.Raycast(transform.position, Vector3.down, out GroundDistHit, Mathf.Infinity))
                {
                    if (GroundDistHit.distance < DistanceFromGround) move = "recoverDive";
                }
            }

            if (TargetInFieldOfView || alwaysAgro)
            {
                if (move == "wait" || move == "0")
                {
                    EyesSetColorTo(colorChase);
                    move = "targetInRange";
                    animator.SetBool("IsChasing", true);
                }
            }
            else
            {
                if (move == "targetInRange")
                {
                    EyesSetColorTo(colorNormal);
                    move = "wait";
                    animator.SetBool("IsChasing", false);
                }
            }

            if (move == "targetInRange")
            {
                if (canUseProjectile && DistancePlayer >= FireRange.x && DistancePlayer <= FireRange.y)
                {
                    if (move != "shoot") move = "shoot";
                }
                if (canUseMelee && DistancePlayerDown <= FallWhenDistance)
                {
                    if (move != "melee")
                    {
                        lastY = transform.position.y;
                        move = "melee";
                        animator.SetTrigger("tCharge");
                        timerGeneral = FallWait;

                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, GroundLayer))
                        {
                            targetPreview.transform.position = hit.point;
                            targetPreview.transform.position += Vector3.up * 0.01f;
                            targetPreview.SetActive(true);
                        }
                    }
                }
                else
                {
                    Vector3 relativePos = new Vector3(CurrentTarget.position.x, transform.position.y, CurrentTarget.position.z) - transform.position;
                    Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.25f);

                    transform.Translate(0, 0, speed.x * Time.deltaTime);
                }
            }
            if (move == "recoverDive")
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), 0.5f);
                transform.Translate(speed.y * Vector3.up * Time.deltaTime);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
                {
                    if (hit.distance >= DistanceFromGround)
                    {
                        animator.SetBool("IsMoving", true);
                        move = "wait";
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    }
                }
                if (transform.position.y > lastY)
                {
                    animator.SetBool("IsMoving", true);
                    move = "wait";
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
            }
            if (move == "shoot")
            {
                Vector3 relativePos = CurrentTarget.position - transform.position;
                Quaternion lookAtTarget = Quaternion.LookRotation(relativePos, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookAtTarget, 0.25f);

                if (Cooldown.Count > 0)
                {
                    if (cooldownAttack <= 0)
                    {
                        animator.SetTrigger("Shoot");
                        currentCooldown += 1;
                        if (Cooldown.Count < currentCooldown + 1) currentCooldown = 0;
                        cooldownAttack = Cooldown[currentCooldown];
                    }
                    else cooldownAttack -= Time.deltaTime;
                }
                else
                {
                    animator.SetTrigger("Shoot");
                }

                if (DistancePlayer < FireRange.x || DistancePlayer > FireRange.y) move = "wait";
            }
            if (move == "wait")
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), 0.07f);
            }
            if (move == "melee")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    rb.useGravity = true;
                    rb.isKinematic = false;

                    rb.AddForce(Vector3.down * addForceDive, ForceMode.Impulse);
                    rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, 20f);

                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsChasing", false);
                }
            }
            if (move == "melee2")
            {
                timerGeneral -= Time.deltaTime;
                if (timerGeneral <= 0)
                {
                    animator.SetBool("Stun", false);
                    ToogleMainAttack(-1);
                }
            }
        }
    }

    void isPlayerInFieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, LookRange, LayerTarget);
        if (rangeChecks.Length > 0)
        {
            bool leureDetected = false;

            for (int i = 0; i < rangeChecks.Length; i++)
            {
                if (rangeChecks[i].CompareTag("Leure"))
                {
                    if (Leure != rangeChecks[i].transform) Leure = rangeChecks[i].transform;
                    leureDetected = true;
                }
                if (rangeChecks[i].CompareTag("Player"))
                {
                    if (Player == null) Player = rangeChecks[i].transform;
                }
            }

            if (leureDetected)
            {
                TargetInFieldOfView = true;
                CurrentTarget = Leure;

                return;
            }
            else if (Player != null)
            {
                TargetInFieldOfView = true;
                CurrentTarget = Player;

                return;
            }
        }

        TargetInFieldOfView = false;
    }

    protected void ShootProjectiles()
    {
        GameObject projectile = Instantiate(Projectile);
        projectile.transform.parent = transform;
        projectile.transform.localPosition = spawnProjectile;
        projectile.transform.rotation = transform.rotation;
        projectile.transform.parent = null;

        projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * 25, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (move == "melee")
        {
            if (((1 << collision.gameObject.layer) & GroundLayer) == 0) return; // ignore player collider

            move = "melee2";
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true; // kinematic immediately = no more collision events while stunned on ground

            animator.SetTrigger("tAttack");
            animator.SetBool("Stun", true);
            timerGeneral = StunTimeRecoverFromAttack;
            ToogleMainAttack(1);
            targetPreview.SetActive(false);
        }
    }   

    public override void TakeDamage(int damage, float stun)
    {
        bool wasStun = move == "stun" || move == "melee2";

        base.TakeDamage(damage, stun);

        if (HP > 0)
        {
            hitVFX.transform.SetParent(transform.parent);
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(CurrentTarget.transform.position.x, hitVFX.transform.position.y, CurrentTarget.transform.position.z);
            hitVFX.transform.LookAt(lookTarget);
            hitVFX.transform.Rotate(0, 90, 0);

            hitVFX.SetActive(true);
            animator.SetTrigger("tHit");

            // DON'T call StunEnnemy here — base.TakeDamage already handles it
            // StunEnnemy was leaving rb active on the ground, causing multi-hit

            animator.SetBool("IsMoving", false);
            animator.SetBool("IsChasing", false);
            animator.ResetTrigger("tCharge");

            if (wasStun) move = "stun";
            else RecoverStun();
        }
    }

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        // Don't interrupt the dive or ground recovery with a stun
        if (move == "melee" || move == "melee2") return;

        base.StunEnnemy(stunTime, infiniteStun);
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    protected override void EndStun()
    {
        animator.SetBool("Stun", false);
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void RecoverStun()
    {
        move = "recoverDive";
        transform.Rotate(0, 180, 0);
    }

    protected override void Death()
    {
        move = "death";
        if (EnnemyManager.Instance != null)
        {
            Debug.Log("remove");
            EnnemyManager.Instance.enemies.Remove(this);
            EnnemyManager.Instance.Check();
        }
        animator.SetBool("IsDead", true);
        OnDeath?.Invoke(this);
    }

    protected override void DeathVFXAppear()
    {
        base.DeathVFXAppear();
        SpriteEnnemy.SetActive(false);
    }
}
