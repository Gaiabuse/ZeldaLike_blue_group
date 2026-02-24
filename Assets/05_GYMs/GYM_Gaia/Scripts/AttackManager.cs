using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class AttackManager : MonoBehaviour
{
    [SerializeField]
    protected SimpleAttack[] comboAttacks;

    [SerializeField] private ManaGauge manaGauge;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [SerializeField] protected float timeForDoCombo;
    [SerializeField]protected PlayerController player;

    [SerializeField] private int ManaAddAtSuccessCombo = 5;
    
    public bool CanAttack;
    private bool canChargedAttack;
    private Attack currentAttack;
    private int currentCombo;
    private Coroutine comboCoroutine;
    private int numberOfAttacksInCombo;
    private bool[] allAttackTouched;
    private void OnEnable()
    {
        CanAttack = true;
        canChargedAttack = false;
        numberOfAttacksInCombo = comboAttacks.Length;
    }

  
    void OnAttack(InputValue _input)
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
    void OnChargedAttack(InputValue _input)
    {
        canChargedAttack = true;
    }

    public void Attack(SimpleAttack attack)
    {
        if (!CanAttack) return;
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }
        currentAttack = attack.Attack(manaGauge, player.transform);
        currentAttack.Finished += AttackIsFinished;
    }

    private void AttackIsFinished(bool touchedEnemy)
    {
        if(currentAttack == null)return;
        if (currentCombo == 0)
        {
            StartCombo();
        }
        allAttackTouched[currentCombo] = touchedEnemy;
        comboCoroutine = StartCoroutine(ComboCoroutine());
        CanAttack = true;
        currentAttack.Finished -= AttackIsFinished;
        currentAttack = null;
    }

    private bool CheckIfAllTouched()
    {
        foreach (bool touched in allAttackTouched)
        {
            if (!touched)
            {
                return false;
            }
        }
        return true;
    }

    private void StartCombo()
    {
        currentCombo = 0;
        allAttackTouched = new bool[numberOfAttacksInCombo];
        for (var i = 0; i < allAttackTouched.Length; i++)
        {
            allAttackTouched[i] = false;
        }
    }

    private IEnumerator ComboCoroutine()
    {
        currentCombo++;
        if (currentCombo >= comboAttacks.Length)
        {
            currentCombo = 0;
            if (CheckIfAllTouched())
            {
                Debug.Log("you success the combo");
            }
        }
        yield return new WaitForSeconds(timeForDoCombo);
        currentCombo = 0;
     
    }
}

[Serializable]
public class SimpleAttack 
{
    [SerializeField]private AttackData AttackData;
    [SerializeField]private Attack.TypeOfAttack type;
    public Attack Attack(ManaGauge manaGauge, Transform player)
    {
        var lAttack = UnityEngine.Object.Instantiate(AttackData.attackPrefab,player);
        lAttack.SetAttack(AttackData, type, manaGauge);
        return lAttack;
    }
}