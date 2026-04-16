
using System;
using System.Collections;
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
    [SerializeField] private float durationUltimate;
    [SerializeField] private float durationChooseEnemyMod;
    [SerializeField] private PlayerInput playerInput;
    
    private AutoAimable[] enemies;
    private AutoAimable currentEnemy;
    private int enemyIndicator;
    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
        playerInput.actions.FindActionMap("NeutralUltMap").Disable();
    }
    
    protected override void OnAttack(InputValue _input)
    {
        base.OnAttack(_input);
        if(erasedManager.startEnemyErased)return;
        Debug.Log(switchInProgress);
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
        Debug.Log("Ultimate");
        ChooseEnemy();
        playerInput.actions.FindActionMap("PlayerControl").Disable();
       
    }
    private void ChooseEnemy()
    {
        enemies =  AutoAimable.GetTargetAround(transform.position, 30f).ToArray();
        foreach (AutoAimable enemy in enemies)
        {
            enemy.GetComponent<EnnemyBase>().StunEnnemy(0.1f,true);
        }
        AutoAimable nearestEnemy = AutoAimable.GetNearestTargetAround(transform.position, 30f);
        enemyIndicator = GetIfEnemyIsInEnemies(nearestEnemy);
        Debug.Log(enemyIndicator);
        if (!playerInput.actions.FindActionMap("NeutralUltMap").enabled)
        {
            playerInput.actions.FindActionMap("NeutralUltMap").Enable();
        }
    }

    private int GetIfEnemyIsInEnemies(AutoAimable nearestEnemy)
    {
        for (var i = 0; i < enemies.Length; i++)
        {
            if (nearestEnemy == enemies[i])
            {
                return i;
            }
        }

        return -1;
    }


    
    public void OnChooseLeft(InputValue _input)
    {
        if (_input.isPressed)
        {
            enemyIndicator--;
            if (enemyIndicator <= 0)
            {
                enemyIndicator = enemies.Length - 1;
            }
            currentEnemy = enemies[enemyIndicator];
            Debug.Log(currentEnemy.name + enemyIndicator);
        }
    }

    public void OnChooseRight(InputValue _input)
    {
        if (_input.isPressed)
        {
            enemyIndicator++;
            if (enemyIndicator >= enemies.Length)
            {
                enemyIndicator = 0;
            }

            currentEnemy = enemies[enemyIndicator];
            Debug.Log(currentEnemy.name + enemyIndicator);
        }
    }
    

    
}
