
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class NeutralAttackManager : AttackManager
{
    [SerializeField] private ErasedManager erasedManager;
    [SerializeField]
    private SimpleAttack[] comboAttacks;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [SerializeField]private float rangeOfUltimate;
    [SerializeField] private LayerMask enemyLayer;
 
    [SerializeField] private PlayerInput playerInput;
    [Header("Ult")]
    [SerializeField] private float durationUltimate;
    [SerializeField]
    private SimpleAttack[] ultimateAttacks;
    [SerializeField]private float knockbackDistance = 2.0f;
    [SerializeField]private float dashOffset = 1.0f;
    [SerializeField]private float ultStun = 2f;
    [SerializeField]private LayerMask groundLayer;
    [SerializeField]private LayerMask obstacleLayer;
    private List<EnnemyBase> enemies = new List<EnnemyBase>();
    private EnnemyBase currentEnemy;
    private int enemyInt;
    private Coroutine ultModCoroutine;
    private Coroutine securityCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
        playerInput.actions.FindActionMap("NeutralUltMap").Disable();
    }
    
    protected override void OnAttack(InputValue _input)
    {
        if (isInUltMod)
        {
            var action = player.playerInput.actions["Attack"];
        
            if (action.activeControl != null)
            {
                string direction = action.activeControl.name; 
                CheckInputDirectionForCancelUltimate(direction);
            }
           
        }
        base.OnAttack(_input);
        var attackAction = player.playerInput.actions["Attack"];
        if (attackAction.activeControl != null)
        {
            string direction = attackAction.activeControl.name;

            if (direction != "buttonNorth")
            {
                return;
            }
        }
        if (!_input.isPressed && switchInProgress)
        {
            if (finishSwitchCoroutine != null)
            {
                StopCoroutine(finishSwitchCoroutine);
            }
            finishSwitchCoroutine = StartCoroutine(FinishSwitch());
        }
        if (switchInProgress)
        {
            return;
        }
        
        if (!_input.isPressed)
        {
            var targetComponent = AutoAimable.GetNearestTargetAround(transform.position, 30f);
    
            if (targetComponent != null)
            {
                Vector3 targetPos = targetComponent.transform.position;
                targetPos.y = transform.parent.position.y;
                transform.parent.LookAt(targetPos);
            }
            player.CanMove = false;
            player.CanRotate = false;
            if (canChargedAttack)
            {
                canChargedAttack = false;
                Attack(ChargedAttack);
                return;
            }
            
            FormAnimator.SetBool("isAttacking",true);
            if (currentCombo >= comboAttacks.Length - 1)
            {
                FormAnimator.SetTrigger("isFinalAttack");
            }
            else
            {
                FormAnimator.SetTrigger("Attack"+currentCombo);
            }
            if(isInUltMod)
            {
                Attack(ultimateAttacks[currentCombo]);
            }
            else
            {
                Attack(comboAttacks[currentCombo]);
            }
            switchInProgress = false;
        }
    }

    private void CheckInputDirectionForCancelUltimate(string direction)
    {
        if (direction != "buttonNorth")
        {
            Debug.Log("Cancel");
            CancelUlt();
        }
    }
    public override void Ultimate()
    {
        base.Ultimate();
        if (ultModCoroutine != null)
        {
            StopCoroutine(ultModCoroutine);
        }
        ultModCoroutine = StartCoroutine(UltModCoroutine());


    }

    
    #region UltMod
    private IEnumerator ChooseEnemy()
    {
        enemies.Clear();
        yield return new WaitForSeconds(0.1f);
        var enemiesAim = AutoAimable.GetTargetAround(transform.position, 30f);
        foreach (AutoAimable enemy in enemiesAim)
        {
            EnnemyBase ennemyBase = enemy.GetComponent<EnnemyBase>();
            if (ennemyBase != null)
            {
                Debug.Log(ennemyBase);
                ennemyBase.StunEnnemy(1000000f,true);
                enemies.Add(ennemyBase);
            }
        }
        if (enemies.Count <= 0)
        {
            CancelUlt();
            yield break;
        }
        AutoAimable nearestEnemy = AutoAimable.GetNearestTargetVisible(transform.position, 30f,groundLayer,obstacleLayer );
        Debug.Log(nearestEnemy);
        if (nearestEnemy != null)
        {
            currentEnemy = nearestEnemy.GetComponent<EnnemyBase>();
        }
        else
        {
            CancelUlt();
            yield break;
        }
        if (securityCoroutine != null)
        {
            StopCoroutine(securityCoroutine);
        }
        securityCoroutine = StartCoroutine(Security());
        UltAttack();
    }

    private IEnumerator Security()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            foreach (var enemy in enemies)
            {
                enemy.StunEnnemy(1000000f,true);
            }
        }
    }
    
    private void UltAttack()
    {
        if (currentEnemy == null) return;
        if (securityCoroutine != null)
        {
            StopCoroutine(securityCoroutine);
        }
        Vector3 directionToEnemy = (currentEnemy.transform.position - transform.position).normalized;
        Vector3 enemyPos = currentEnemy.transform.position - (directionToEnemy * dashOffset);
        player.Teleport(enemyPos); 
        
        foreach (EnnemyBase enemy in enemies)
        {
            if (enemy != null && enemy != currentEnemy)
            {
                ApplyKnockback(enemy);
            }
        }

        if (isInUltMod)
        {
            currentEnemy.OnDeath += ChooseEnemyAfterDeath;
            currentEnemy.SetUltIndicator(true);
        }
    }

    private IEnumerator UltModCoroutine()
    {
        isInUltMod = true;
        StartCoroutine(ChooseEnemy());
        yield return new WaitForSeconds(durationUltimate);
        isInUltMod = false;
        if (currentEnemy)
        {
            currentEnemy.OnDeath -= ChooseEnemyAfterDeath;
            currentEnemy.SetUltIndicator(false);
        }
        foreach (EnnemyBase enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.StunEnnemy(0.05f,false);
            }
        }
        
    }
    private void ChooseEnemyAfterDeath(EnnemyBase enemy)
    {
        if(!isInUltMod)return;
        StartCoroutine(ChooseEnemy());
    }

    private void ApplyKnockback(EnnemyBase target)
    {
        Vector3 pushDirection = (target.transform.position - transform.position).normalized;
        
        pushDirection.y = 0;
        
        target.transform.position += pushDirection * knockbackDistance;
    }
    
    private void CancelUlt()
    {
        isInUltMod = false;
        if (securityCoroutine != null) StopCoroutine(securityCoroutine);
        if (ultModCoroutine != null) StopCoroutine(ultModCoroutine);
        foreach (EnnemyBase enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.StunEnnemy(0.05f,false);
            }
        }
    }
    #endregion
}
    

