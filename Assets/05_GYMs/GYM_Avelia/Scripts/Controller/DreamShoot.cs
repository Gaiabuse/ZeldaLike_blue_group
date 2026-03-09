using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class DreamShoot : AttackManager
{
    [SerializeField]
    Projectile attack;

    [SerializeField]
    PlayerController controller;

    [SerializeField]
    private ManaGauge manaGauge;

    [SerializeField]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f;

    [SerializeField]
    Transform SpawnPoint;

    [SerializeField]
    protected Attack.TypeOfAttack type;
    [SerializeField]
    protected AttackData data;

    [SerializeField]
    private AnimationCurve ChargedPowerEvolution;

    [SerializeField]
    private float MaxChargedTime;
    [SerializeField]
    private float minAttack, maxAttack;

    public float MinAttack => minAttack;
    public float MaxAttack => maxAttack;
    [SerializeField] private int numberOfShotsForUltimate;


    float lastInputTime;

    protected override void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            lastInputTime = Time.time;
            player.CanMove = false;
            // we should try to do something to make things seem more sensitive

            aimCone.SetActive(true);

            var playerPos = player.transform.position;
            var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);

            if (AutoAimed != null)
                player.transform.LookAt(AutoAimed.transform, Vector3.up);
            return;
        }

        player.CanMove = true;
        aimCone.SetActive(false);
        var amountOfTimeWaited = Time.time - lastInputTime;

        var progress = amountOfTimeWaited / MaxChargedTime;
        progress = Mathf.Min(progress, 1f);

        var attackScaledPower = GetAttackPower(progress);

        if (amountOfTimeWaited < autoAimTime)
        {
            CreateAutoTargettingShot(attackScaledPower);
            return;
        }

        CreateShot(attackScaledPower);
        return;
    }


    public override void Ultimate()
    {
        Quaternion LastRotation = player.transform.rotation;
        for (int i = 0; i < numberOfShotsForUltimate; i++)
        {
            float positionY = (360f / numberOfShotsForUltimate) * i;
            player.transform.rotation = Quaternion.Euler(0, positionY, 0);
            CreateShot(maxAttack);
        }
        player.transform.rotation = LastRotation;

    }

    void CreateShot(float attackPower)
    {
        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(attackPower, data, type, manaGauge);
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = player.transform.forward * ProjectileSpeed;
    }

    void CreateAutoTargettingShot(float AttackPower)
    {
        // do shit
        var playerPos = player.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);
        if (AutoAimed == null)
        {
            CreateShot(AttackPower);
            return;
        }
        player.transform.LookAt(AutoAimed.transform, Vector3.up);



        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(AttackPower, data, type, manaGauge);
        lAttack.transform.position = playerPos + directionToGo * offset;
        lAttack.speed = directionToGo * ProjectileSpeed;
        lAttack.GetComponent<ScalingAttack>().SetMinMax(minAttack, maxAttack);
    }

    float GetAttackPower(float proggression)
        => ChargedPowerEvolution.Evaluate(proggression) * (maxAttack - minAttack) + minAttack;

}
