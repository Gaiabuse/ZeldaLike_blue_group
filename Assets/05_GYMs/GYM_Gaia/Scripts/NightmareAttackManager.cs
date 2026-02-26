
using System;
using System.Collections;
using UnityEngine;

public class NightmareAttackManager : AttackManager
{
    [SerializeField] private GameObject[] playerObjects;
    [SerializeField] private GameObject ultimateObject;

    [SerializeField] private SimpleAttack ultimateAttack;
    [SerializeField] private float timeOfUltimate;
    private void Awake()
    {
        ultimateObject.SetActive(false);
    }

    public override void Ultimate()
    {
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
