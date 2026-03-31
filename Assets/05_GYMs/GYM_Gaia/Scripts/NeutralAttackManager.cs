
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class NeutralAttackManager : AttackManager
{
    [SerializeField]
    private SimpleAttack[] comboAttacks;
    [SerializeField]
    protected SimpleAttack ChargedAttack;

    [Header("Ultimate")]
    [SerializeField]
    private LayerMask enemyLayer;
    [Tooltip("In Senconds")]
    [SerializeField]
    private float ultimateDuration;

    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
    }

    protected override void OnAttack(InputValue _input)
    {
        base.OnAttack(_input);
        Debug.Log(switchInProgress);

        if (!_input.isPressed && switchInProgress)
        {
            if (finishSwitchCoroutine != null)
            {
                StopCoroutine(finishSwitchCoroutine);
            }
            finishSwitchCoroutine = StartCoroutine(FinishSwitch());
        }

        if (switchInProgress) { return; }

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

            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;
        }
    }

    public override void Ultimate()
    {
        base.Ultimate();
        Debug.Log("Ultimate");
        var meow = StartCoroutine(UltimateActivation());
    }

    private IEnumerator UltimateActivation()
    {
        float timer = 0f;

        float ultiDuration = ultimateDuration;

        while (timer < ultimateDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
