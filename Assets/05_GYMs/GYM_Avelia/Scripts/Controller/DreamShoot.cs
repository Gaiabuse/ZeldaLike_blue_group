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
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f;

    [SerializeField]
    Transform SpawnPoint;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField] protected AttackData data;

    [SerializeField] private int numberOfShotsForFinishCombo;
    [SerializeField] private int numberOfShotsForUltimate;


    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = numberOfShotsForFinishCombo;
    }
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

            if(AutoAimed != null)
                player.transform.LookAt(AutoAimed.transform, Vector3.up);
            return;
        }

        player.CanMove = true;
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
    

    public override void Ultimate()
    {
        base.Ultimate();
        Quaternion LastRotation = player.transform.rotation;
        for (int i = 0; i < numberOfShotsForUltimate; i++)
        {
            float positionY = (360f/numberOfShotsForUltimate)*i;
            player.transform.rotation = Quaternion.Euler(0, positionY, 0);
            CreateShot();
        }
        player.transform.rotation = LastRotation;
        
    }

    void CreateShot( )
    {
        Projectile lAttack = Instantiate<Projectile>(attack);

        Attack attackPrefab = lAttack.GetComponent<Attack>();
        attackPrefab.SetAttack(data, type, manaGauge);
        currentAttack = attackPrefab;
        currentAttack.Finished += AttackIsFinished;
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = player.transform.forward* ProjectileSpeed;
    }

    void CreateAutoTargettingShot()
    {
        // do shit
        var playerPos = player.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);
        if (AutoAimed == null)
        {
            CreateShot();
            return;
        }
        player.transform.LookAt(AutoAimed.transform, Vector3.up);

     

        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        Projectile lAttack = Instantiate<Projectile>(attack);

        Attack attackPrefab = lAttack.GetComponent<Attack>();
        attackPrefab.SetAttack(data, type, manaGauge);
        currentAttack = attackPrefab;
        currentAttack.Finished += AttackIsFinished;
        lAttack.transform.position = playerPos + directionToGo * offset;
        lAttack.speed = directionToGo * ProjectileSpeed;
    }

}
