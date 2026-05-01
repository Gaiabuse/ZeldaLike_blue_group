using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DreamShoot : AttackManager
{
    [SerializeField]
    [Tooltip("prefab of the attack")]
    Projectile attack;

    [SerializeField]
    PlayerController controller;

    [SerializeField]
    [Tooltip("Visual for see aim (in prefab)")]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f, coolDown = 0.1f;

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

    [SerializeField] private int numberOfShotsForFinishCombo;
    [SerializeField] private int numberOfShotsForUltimate;

    private bool CanShoot = true;
    private bool prepShoot = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = numberOfShotsForFinishCombo;
    }
    float lastInputTime;

    protected override void OnAttack(InputValue _input)
    {
        base.OnAttack(_input);
        if (!_input.isPressed&& switchInProgress)
        {
            if (finishSwitchCoroutine != null)
            {
                StopCoroutine(finishSwitchCoroutine);
            }
            finishSwitchCoroutine = StartCoroutine(FinishSwitch());
        }
        if (switchInProgress)
        {
            UnprepShoot();
            return;
        }
        if (_input.isPressed)
        {
            var action = player.playerInput.actions["Attack"];
            if (action.activeControl != null)
            {
                string dir = action.activeControl.name;
                if (dir != "buttonWest")
                {
                    UnprepShoot();
                    base.OnAttack(_input);
                    return; 
                }
            }
        
            PrepareShoot();
            return;
        }

        if (!_input.isPressed && !CanShoot)
        {
            UnprepShoot();
            return;
        }

        if (!CanShoot) return;

        switchInProgress = false;
        StartCoroutine(DoShoot());
    }

    private void FixedUpdate()
    {
        player.CanMove = !prepShoot;
    }

    private void PrepareShoot()
    {
        lastInputTime = Time.time;
        prepShoot = true;
        player.CanMove = false;
        player.CanRotate = true;
        // we should try to do something to make things seem more sensitive

        aimCone.SetActive(true);

        var playerPos = player.transform.position;
        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);

        if (AutoAimed != null)
            player.transform.LookAt(AutoAimed.transform, Vector3.up);
    }

    public void UnprepShoot()
    {
        player.CanMove = true;
        prepShoot = false;
        aimCone.SetActive(false);
    }

    public System.Collections.IEnumerator DoShoot()
    {
        player.CanMove = true;
        prepShoot = false;
        aimCone.SetActive(false);

        var amountOfTimeWaited = Time.time - lastInputTime;

        var progress = amountOfTimeWaited / MaxChargedTime;
        progress = Mathf.Min(progress, 1f);

        var attackScaledPower = GetAttackPower(progress);

        if (amountOfTimeWaited < autoAimTime)
            CreateAutoTargettingShot(attackScaledPower);
        else CreateShot(attackScaledPower);

        CanShoot = false;
        yield return new WaitForSeconds(coolDown);
        CanShoot = true;
    }

    public override void Ultimate()
    {
        base.Ultimate();
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

        Attack attackPrefab = lAttack.GetComponent<Attack>();
        attackPrefab.SetAttack(attackPower, data, type);

        currentAttack = attackPrefab;
        currentAttack.Finished += AttackIsFinished;

        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = player.transform.forward * ProjectileSpeed;

        lAttack.GetComponent<ScalingAttack>().SetMinMax(minAttack, maxAttack);
    }

    void CreateAutoTargettingShot(float attackPower)
    {
        // do shit
        var playerPos = player.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);
        if (AutoAimed == null)
        {
            CreateShot(attackPower);
            return;
        }
        player.transform.LookAt(AutoAimed.transform, Vector3.up);

        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        Projectile lAttack = Instantiate<Projectile>(attack);

        Attack attackPrefab = lAttack.GetComponent<Attack>();

        attackPrefab.SetAttack(attackPower, data, type);
        currentAttack = attackPrefab;
        currentAttack.Finished += AttackIsFinished;

        lAttack.transform.position = playerPos + directionToGo * offset;
        lAttack.speed = directionToGo * ProjectileSpeed;

        lAttack.GetComponent<ScalingAttack>().SetMinMax(minAttack, maxAttack);
    }

    float GetAttackPower(float proggression)
        => MathF.Round(ChargedPowerEvolution.Evaluate(proggression) * (maxAttack - minAttack) + minAttack);

    void OnDisable()
    {
        player.CanMove = true;
        aimCone.SetActive(false);
        CanShoot = true;
    }

}
