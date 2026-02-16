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

    [SerializeField]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f;

    [SerializeField]
    Transform SpawnPoint;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField] protected float damage;



    float lastInputTime;

    void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            Debug.Log("is pressed");
            lastInputTime = Time.time;
            controller.CanMove = false;
            // we should try to do something to make things seem more sensitive

            aimCone.SetActive(true);
            return;
        }

        Debug.Log("unpressed");
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

        lAttack.GetComponent<Attack>().SetAttack(damage, type);
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = controller.transform.forward * ProjectileSpeed;
    }

    void CreateAutoTargettingShot()
    {
        // do shit
        var playerPos = controller.transform.position;
        var overlaps = Physics.OverlapSphere(playerPos, autoAimRadius);

        var AutoAimed = overlaps.Select(a => a.GetComponent<AutoAimable>())
            .Where(a => !(a == null))
            .OrderBy(a => Vector3.Distance(playerPos, a.transform.position) * a.weight)
            .First();

        if (AutoAimed == null)
        {
            CreateShot();
            return;
        }

        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(damage, type);
        lAttack.transform.position = playerPos + directionToGo * offset;
        lAttack.speed = directionToGo * ProjectileSpeed;
    }
}
