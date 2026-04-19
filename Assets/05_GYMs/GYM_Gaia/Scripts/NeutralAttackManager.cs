
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
    [SerializeField] private float durationChooseEnemyMod;
    [SerializeField] private GameObject targetIndicator;
    [SerializeField]private float knockbackDistance = 2.0f;
    [SerializeField]private float dashOffset = 1.0f;
    [SerializeField]private int ultDamage = 15;
    [SerializeField]private float ultStun = 2f;
    private List<EnnemyBase> enemies = new List<EnnemyBase>();
    private EnnemyBase currentEnemy;
    private int enemyInt;
    private Coroutine chooseEnemyCoroutine;
    private Coroutine ultModCoroutine;
    private Coroutine securityCoroutine;
    private bool isInUltMod = false;
 
    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
        playerInput.actions.FindActionMap("NeutralUltMap").Disable();
        targetIndicator.SetActive(false);
    }
    
    protected override void OnAttack(InputValue _input)
    {
        base.OnAttack(_input);
        if(erasedManager.startEnemyErased)return;
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
            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;
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
    private IEnumerator ChooseEnemyCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        ChooseEnemy();
        yield return new WaitForSeconds(durationChooseEnemyMod);
        UltAttack();
        SetNeutralUltMap(false);
    }

    private void SetNeutralUltMap(bool isChooseMod)
    {
        if (isChooseMod)
        {
            playerInput.actions.FindActionMap("PlayerControl").Disable();
            if (!playerInput.actions.FindActionMap("NeutralUltMap").enabled)
            {
                playerInput.actions.FindActionMap("NeutralUltMap").Enable();
            }
        }
        else
        {
            playerInput.actions.FindActionMap("NeutralUltMap").Disable();
            if (!playerInput.actions.FindActionMap("PlayerControl").enabled)
            {
                playerInput.actions.FindActionMap("PlayerControl").Enable();
            }
        }
     
    }
    private void ChooseEnemy()
    {
        enemies.Clear();
        var enemiesAim = AutoAimable.GetTargetAround(transform.position, 30f);
        foreach (AutoAimable enemy in enemiesAim)
        {
            Debug.Log(enemy);
            EnnemyBase ennemyBase = enemy.GetComponent<EnnemyBase>();
            if (ennemyBase != null)
            {
                ennemyBase.StunEnnemy(1000000f,true);
                enemies.Add(ennemyBase);
            }
        }
        if (enemies.Count <= 0)
        {
            CancelUlt();
            return;
        }
        SetNeutralUltMap(true);
        AutoAimable nearestEnemy = AutoAimable.GetNearestTargetAround(transform.position, 30f);
        if (nearestEnemy != null) enemyInt = GetIfEnemyIsInEnemies(nearestEnemy.GetComponent<EnnemyBase>());
        currentEnemy = enemies[enemyInt].GetComponent<EnnemyBase>();
        SetPosTargetIndicator();
        if (securityCoroutine != null)
        {
            StopCoroutine(securityCoroutine);
        }
        securityCoroutine = StartCoroutine(Security());
    }

    private IEnumerator Security()
    {
        while (true)
        {
            SetPosTargetIndicator();
            yield return new WaitForSeconds(0.5f);
            foreach (var enemy in enemies)
            {
                enemy.StunEnnemy(1000000f,true);
            }
        }
    }

    
    private void SetPosTargetIndicator()
    {
        if (targetIndicator == null) return;
        if(!targetIndicator.activeInHierarchy)targetIndicator.SetActive(true);
        if (currentEnemy)
        {
            targetIndicator.transform.position = new Vector3(currentEnemy.transform.position.x, targetIndicator.transform.position.y, currentEnemy.transform.position.z);
        }
       
        
    }
    

    private int GetIfEnemyIsInEnemies(EnnemyBase nearestEnemy)
    {
        for (var i = 0; i < enemies.Count; i++)
        {
            if (nearestEnemy == enemies[i])
            {
                return i;
            }
        }

        return -1;
    }
    
    
    private void UltAttack()
    {
        if (currentEnemy == null) return;
        if (securityCoroutine != null)
        {
            StopCoroutine(securityCoroutine);
        }
        targetIndicator.SetActive(false);
        Vector3 directionToEnemy = (currentEnemy.transform.position - transform.position).normalized;
        Vector3 enemyPos = currentEnemy.transform.position - (directionToEnemy * dashOffset);
        player.Teleport(enemyPos); 
        
        foreach (EnnemyBase enemy in enemies)
        {
            if (enemy != null && enemy != currentEnemy)
            {
                ApplyKnockback(enemy);
                enemy.StunEnnemy(0.05f,false);
            }
        }

        if (isInUltMod)
        {
            currentEnemy.OnDeath += ChooseEnemyAfterDeath;
            currentEnemy.SetUltIndicator(true);
        }
        currentEnemy.TakeDamage(ultDamage,ultStun);
    }

    private IEnumerator UltModCoroutine()
    {
        isInUltMod = true;
        if (chooseEnemyCoroutine != null)
        {
            StopCoroutine(chooseEnemyCoroutine);
        }
        chooseEnemyCoroutine = StartCoroutine(ChooseEnemyCoroutine());
        formSwitcher.canSwitchForm = false;
        yield return new WaitForSeconds(durationUltimate);
        formSwitcher.canSwitchForm = true;
        isInUltMod = false;
        if (currentEnemy)
        {
            currentEnemy.OnDeath -= ChooseEnemyAfterDeath;
            currentEnemy.SetUltIndicator(false);
        }
    }
    private void ChooseEnemyAfterDeath(EnnemyBase enemy)
    {
        if(!isInUltMod)return;
        if (chooseEnemyCoroutine != null)
        {
            StopCoroutine(chooseEnemyCoroutine);
        }
        chooseEnemyCoroutine = StartCoroutine(ChooseEnemyCoroutine());
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
        formSwitcher.canSwitchForm = true;
        if (securityCoroutine != null) StopCoroutine(securityCoroutine);
        if (chooseEnemyCoroutine != null) StopCoroutine(chooseEnemyCoroutine);
        if (ultModCoroutine != null) StopCoroutine(ultModCoroutine);
        if (targetIndicator != null) targetIndicator.SetActive(false);
        SetNeutralUltMap(false);
    }
    public void OnChooseLeft(InputValue _input)
    {
        if (!_input.isPressed && enemies.Count <=0) return;
        enemyInt--;
        if (enemyInt < 0)
        {
            enemyInt = enemies.Count - 1;
        }
        currentEnemy = enemies[enemyInt];
        SetPosTargetIndicator();
    }

    public void OnChooseRight(InputValue _input)
    {
        if (!_input.isPressed&& enemies.Count <=0) return;
        enemyInt++;
        if (enemyInt >= enemies.Count)
        {
            enemyInt = 0;
        }
        currentEnemy = enemies[enemyInt];
        SetPosTargetIndicator();
    }
    #endregion
}
    

