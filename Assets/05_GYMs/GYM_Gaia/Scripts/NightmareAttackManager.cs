
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
        UltimateActivation(true);
        StartCoroutine(UltimateCoroutine());
    }
    private void UltimateActivation(bool isActive)
    {
        ultimateObject.SetActive(isActive);
        CanAttack = !isActive;
        foreach (var go in playerObjects)
        {
            go.SetActive(!isActive);
        }
        if (isActive == false)
        {
            Attack(ultimateAttack);
        }
    }

    private IEnumerator UltimateCoroutine()
    {
        formSwitcher.canSwitchForm = false;
        yield return new WaitForSeconds(timeOfUltimate);
        formSwitcher.canSwitchForm = true;
        UltimateActivation(false);
        Attack(ultimateAttack);
    }
}
