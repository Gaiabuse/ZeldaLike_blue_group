using UnityEngine;
using System.Collections.Generic;

public class FlyingEnnemy : EnnemyBase
{
    Rigidbody rb;

    [Header("Flying Ennemy")]
    [SerializeField] float LookRange = 12f;
    [SerializeField] float DistanceFromGround = 5;

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

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        if (Cooldown.Count > 0) cooldownAttack = Cooldown[currentCooldown];
        else cooldownAttack = 0;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        isPlayerInFieldOfView();

        if (move != "stun")
        {
            float DistancePlayer = Vector3.Distance(transform.position, CurrentTarget.position);
            float DistancePlayerDown = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(CurrentTarget.position.x, CurrentTarget.position.z));

            if (TargetInFieldOfView || alwaysAgro)
            {
                if (move == "wait")
                {
                    EyesSetColorTo(colorChase);
                    move = "targetInRange";
                }
            }
            else
            {
                if (move == "targetInRange")
                {
                    EyesSetColorTo(colorNormal);
                    move = "wait";
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
                        move = "melee";
                        SetDive(1);
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
                        move = "wait";
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    }
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
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(0, 5);
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

    void SetDive(int dive)
    {
        animator.SetInteger("Dive", dive);
        if (dive == 0 && move == "melee2")
        {
            move = "recoverDive";
        }
        if (dive == 2 && move == "melee")
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            rb.AddForce(Vector3.down * addForceDive, ForceMode.Impulse);
            ToogleMainAttack(1);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (move == "melee")
        {
            move = "melee2";
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
            SetDive(3);
            ToogleMainAttack(-1);
        }
    }

    public override void StunEnnemy(float stunTime, bool infiniteStun)
    {
        base.StunEnnemy (stunTime, infiniteStun);
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    protected override void EndStun()
    {
        base.EndStun();
        rb.isKinematic = true;
        rb.useGravity = false;
        move = "recoverDive";
    }
}
