using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DreamShoot : MonoBehaviour
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


    float lastInputTime;

    void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            lastInputTime = Time.time;
            controller.CanMove = false;
            // we should try to do something to make things seem more sensitive

            aimCone.SetActive(true);

            var playerPos = controller.transform.position;
            var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);

            controller.transform.LookAt(AutoAimed.transform, Vector3.up);
            return;
        }

        controller.CanMove = true;
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

    void CreateShot(float attackPower)
    {
        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(attackPower, data, type, manaGauge);
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = controller.transform.forward * ProjectileSpeed;
    }

    void CreateAutoTargettingShot(float AttackPower)
    {
        // do shit
        var playerPos = controller.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);

        controller.transform.LookAt(AutoAimed.transform, Vector3.up);

        if (AutoAimed == null)
        {
            CreateShot(AttackPower);
            return;
        }

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
