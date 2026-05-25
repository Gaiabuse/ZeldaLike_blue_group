
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.VFX;

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
    [SerializeField] private GameObject ultZone;
    [SerializeField]
    private SimpleAttack[] ultimateAttacks;
    [SerializeField]private float knockbackDistance = 2.0f;
    [SerializeField]private float dashOffset = 1.0f;
    [SerializeField]private float ultStun = 2f;
    [SerializeField]private float ultRadius = 3f;
    [SerializeField]private VisualEffect ultVFX;
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

            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;

            FormAnimator.SetBool("isAttacking", true);
            FormAnimator.SetTrigger("Attack" + currentCombo);
            Debug.Log("Attack" + currentCombo);
        }
    }


    public override void Ultimate()
    {
        base.Ultimate();
        StartUlt();
    }

    
    #region UltMod

    private void StartUlt()
    {
        player.CanMove = false;
        player.CanRotate = false;
        isInUltMod = true;
        ultVFX.enabled = true;

        var enemiesAim = AutoAimable.GetTargetAround(transform.position, ultRadius);
        foreach (AutoAimable enemy in enemiesAim)
        {
            EnnemyBase ennemyBase = enemy.GetComponent<EnnemyBase>();
            if (ennemyBase != null)
            {
                ennemyBase.StunEnnemy(ultStun,false);
            }
        }
        player.CanMove = true;
        player.CanRotate = true;
        isInUltMod = false;
        if (currentEnemy)
        {
            currentEnemy.SetUltIndicator(false);
        }
    }
    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, ultRadius);
    }
    #endregion
}
    

