
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
        
        if (_input.isPressed)
        {
            Attack(comboAttacks[currentCombo]);
            return;
        }
        
        if (canChargedAttack)
        {
            canChargedAttack = false;
            Attack(ChargedAttack);
        }
    }


    public override void Ultimate()
    {
        base.Ultimate();
        Debug.Log("Ultimate");
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
        yield return new WaitForSeconds(timeOfUltimate);
        UltimateActivation(false);
        Attack(ultimateAttack);
    }
}
