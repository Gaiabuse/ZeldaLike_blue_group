
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NightmareAttackManager : AttackManager
{
    [SerializeField]
    private SimpleAttack[] comboAttacks;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [SerializeField] private GameObject[] playerObjects;
    [SerializeField] private GameObject ultimateObject;

    [SerializeField] private SimpleAttack ultimateAttack;
    [SerializeField] private float timeOfUltimate;
    
    private Coroutine ultimateCoroutine;
    private void Awake()
    {
        ultimateObject.SetActive(false);
    }
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
            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;
        }
    }


    public override void Ultimate()
    {
        base.Ultimate();
        UltimateActivation();
        if (ultimateCoroutine != null)
        {
            StopCoroutine(ultimateCoroutine);
        }
        ultimateCoroutine = StartCoroutine(UltimateCoroutine());
    }
    private void UltimateActivation()
    {
        formSwitcher.canSwitchForm = false;
        CanAttack = false;
        ultimateObject.SetActive(true);
        foreach (var go in playerObjects)
        {
            go.SetActive(!false);
        }
    }

    private void UltimateDesactivation()
    {
        formSwitcher.canSwitchForm = true;
        CanAttack = true;
        ultimateObject.SetActive(false);
        foreach (var go in playerObjects)
        {
            go.SetActive(true);
        }
        Attack(ultimateAttack);
    }

    private IEnumerator UltimateCoroutine()
    {
        yield return new WaitForSeconds(timeOfUltimate);
        formSwitcher.canSwitchForm = true;
        UltimateDesactivation();
        ultimateCoroutine = null;
    }
}
