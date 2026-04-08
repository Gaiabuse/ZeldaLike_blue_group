using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class FlyingEnnemy : EnnemyBase
{
    [Header("Flying Ennemy")]
    [SerializeField] float LookRange = 12f;

    [Header("Melee Setting")]
    [SerializeField] bool canUseMelee = true;
    [SerializeField] float FallWhenDistance = 0.5f;

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
        if (Cooldown.Count > 0) cooldownAttack = Cooldown[currentCooldown];
        else cooldownAttack = 0;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        isPlayerInFieldOfView();

        if (move != "stun")
        {
            if (TargetInFieldOfView)
            {
                if (move != "targetInRange")
                {
                    EyesSetColorTo(colorChase);
                    move = "targetInRange";
                }
            }
            else
            {
                if (move != "wait")
                {
                    EyesSetColorTo(colorNormal);
                    move = "wait";
                }
            }

            if (move == "targetInRange")
            {
                float DistancePlayer = Vector3.Distance(transform.position, CurrentTarget.position);

                if (canUseProjectile && DistancePlayer <= FireRange.y && DistancePlayer >= FireRange.x)
                {
                    if (move != "shoot") move = "shoot";
                }
                if (canUseMelee)
                {

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
            }
            if (move == "wait")
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), 0.07f);
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
}
