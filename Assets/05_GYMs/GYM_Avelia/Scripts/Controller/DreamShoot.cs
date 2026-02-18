using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class DreamShoot : MonoBehaviour
{
    [SerializeField]
    Projectile attack;

    [SerializeField]
    PlayerController controller;
    [SerializeField] private ManaGauge manaGauge;
    [SerializeField]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f;

    [SerializeField]
    Transform SpawnPoint;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField] protected AttackData data;



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

        if (amountOfTimeWaited < autoAimTime)
        {
            CreateAutoTargettingShot();
            return;
        }

        CreateShot();
        return;
    }

    void CreateShot()
    {
        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(data, type, manaGauge);
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = controller.transform.forward * ProjectileSpeed;
    }

    void CreateAutoTargettingShot()
    {
        // do shit
        var playerPos = controller.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);

        controller.transform.LookAt(AutoAimed.transform, Vector3.up);

        if (AutoAimed == null)
        {
            CreateShot();
            return;
        }

        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(data, type, manaGauge);
        lAttack.transform.position = playerPos + directionToGo * offset;
        lAttack.speed = directionToGo * ProjectileSpeed;
    }

}
