using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

public class DreamShoot : AttackManager
{
    [SerializeField]
    [Tooltip("prefab of the attack")]
    Projectile attack;

    [SerializeField]
    [Tooltip("prefab of the attack")]
    Projectile ultimateAttackOfCombo;

    [SerializeField]
    PlayerController controller;

    [SerializeField]
    [Tooltip("Visual for see aim (in prefab)")]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f, autoAimRadius = 3, offset = 0.2f, cooldownFinishShoot = 1f;

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

    [SerializeField]
    private int numberOfShotsForFinishCombo;
    [SerializeField]
    private int numberOfShotsForUltimate;
    [SerializeField]
    private float lastAttackComboCoolDown = 0.5f;
    [SerializeField]
    private float lastAttackComboDamage = 10f;

    [SerializeField] private bool CanShoot = true;
    private bool prepShoot = false;


    public bool IsLastComboAttack => currentCombo >= numberOfAttacksInCombo-1;
    private Projectile CurrentProjectile => IsLastComboAttack ? ultimateAttackOfCombo : attack;

    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = numberOfShotsForFinishCombo;

        // Force-reset shooting safety guards when entering this form
        CanShoot = true;
        prepShoot = false;
        switchInProgress = false;
        currentCombo = 0; // Make sure combo starts at zero
    }
    float lastInputTime;
    

    protected override void OnAttack(InputValue _input)
    {
        // Pass the input to the base method safely
        base.OnAttack(_input);

        // 1. BUTTON RELEASED
        if (!_input.isPressed)
        {
            // Trigger the release animation
            if (base.FormAnimator != null)
            {
                base.FormAnimator.ResetTrigger("isMaintainingButton"); // Clean up the hold trigger just in case
                base.FormAnimator.SetTrigger("Attack0");
            }

            if (switchInProgress)
            {
                if (finishSwitchCoroutine != null)
                    StopCoroutine(finishSwitchCoroutine);
                else
                    finishSwitchCoroutine = StartCoroutine(FinishSwitch());
            }

            if (switchInProgress || !CanShoot)
            {
                UnprepShoot();
                return;
            }

            // Fire the projectile
            switchInProgress = false;
            StartCoroutine(DoShoot());
        }
        // 2. BUTTON PRESSED / HELD
        else 
        {
            // Trigger the holding/charging animation
            if (base.FormAnimator != null)
            {
                base.FormAnimator.ResetTrigger("Attack0"); // Clean up previous fire triggers
                base.FormAnimator.SetTrigger("isMaintainingButton");
            }

            if (switchInProgress)
            {
                UnprepShoot();
                return;
            }

            var action = player.playerInput.actions["Attack"];
            if (action.activeControl != null)
            {
                string dir = action.activeControl.name;
                if (dir != "buttonWest")
                {
                    UnprepShoot();
                    return;
                }
            }

            PrepareShoot();
        }
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

        float attackScaledPower;

        if (IsLastComboAttack)
        {
            print("DoLastComboAttack");
            attackScaledPower = lastAttackComboDamage;
        }
        else
        {
            var progress = amountOfTimeWaited / MaxChargedTime;
            progress = Mathf.Min(progress, 1f);

            attackScaledPower = GetAttackPower(progress);
        }


        if (amountOfTimeWaited < autoAimTime)
            CreateAutoTargettingShot(attackScaledPower);
        else CreateShot(attackScaledPower, transform.forward, CurrentProjectile);

        bool wasLastCombo = IsLastComboAttack;
        
        Combo();
        CanShoot = false;
        StartCoroutine(FinishShoot(wasLastCombo));
        yield return null;
    }
    
    public IEnumerator FinishShoot(bool wasLastCombo)
    {
        var cooldown = wasLastCombo ? lastAttackComboCoolDown : cooldownFinishShoot;
        yield return new WaitForSeconds(cooldown);
        
        CanShoot = true; 
        FinishAttack();
    }



    public override void Ultimate()
    {
        base.Ultimate();
        base.FormAnimator.SetTrigger("usingAtkSpe");
        Quaternion LastRotation = player.transform.rotation;
        for (int i = 0; i < numberOfShotsForUltimate; i++)
        {
            float positionY = (360f / numberOfShotsForUltimate) * i;
            player.transform.rotation = Quaternion.Euler(0, positionY, 0);
            CreateShot(lastAttackComboDamage, transform.forward, attack);
        }
        player.transform.rotation = LastRotation;
    }

    void CreateShot(float attackPower, Vector3 direction, Projectile Shot)
    {
        Projectile lAttack = Instantiate<Projectile>(Shot);

        Attack attackPrefab = lAttack.GetComponent<Attack>();
        attackPrefab.SetAttack(attackPower, data, type);

        currentAttack = attackPrefab;

        lAttack.transform.position = transform.position + direction * offset;
        lAttack.speed = direction * ProjectileSpeed;

        ScalingAttack scale = lAttack.GetComponent<ScalingAttack>();
        if (scale != null)
        {
            lAttack.GetComponent<ScalingAttack>().SetMinMax(minAttack, maxAttack);
        }
    }

    void CreateAutoTargettingShot(float attackPower)
    {
        var playerPos = player.transform.position;

        var AutoAimed = AutoAimable.GetNearestTargetAround(playerPos, autoAimRadius);
        if (AutoAimed == null)
        {
            CreateShot(attackPower, transform.forward, CurrentProjectile);
            return;
        }
        player.transform.LookAt(AutoAimed.transform, Vector3.up);

        var ToGoTo = AutoAimed.transform.position;
        var directionToGo = (ToGoTo - playerPos).normalized;

        CreateShot(attackPower, directionToGo, CurrentProjectile);
    }

    float GetAttackPower(float proggression)
        => MathF.Round(ChargedPowerEvolution.Evaluate(proggression) * (maxAttack - minAttack) + minAttack);

    void OnDisable()
    {
        // Turn off the aim UI instantly so it doesn't get stuck on screen
        if (aimCone != null)
            aimCone.SetActive(false);

        if (player != null)
        {
            player.CanMove = true;
            player.CanRotate = true;
        }

        CanShoot = true;
        prepShoot = false;
        switchInProgress = false;
        currentCombo = 0;

        if (ultimateCoroutine != null)
        {
            StopCoroutine(ultimateCoroutine);
            ultimateCoroutine = null;
        }
 
        StopAllCoroutines(); 
    }



}
