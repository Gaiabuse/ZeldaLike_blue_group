using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class AttackManager : MonoBehaviour
{
    [SerializeField]
    private SimpleAttack[] comboAttacks;

    [SerializeField] protected ManaGauge manaGauge;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [SerializeField] protected float timeForDoCombo;
    [SerializeField]protected PlayerController player;

    [SerializeField] private int ManaAddAtSuccessCombo = 5;
    [SerializeField] private float timeForDoUltimate = 2;
    [SerializeField] protected FormSwitcher formSwitcher;
    public bool CanAttack;
    private bool canChargedAttack;
    private Attack currentAttack;
    protected int currentCombo;
    protected Coroutine comboCoroutine;
    protected int numberOfAttacksInCombo;
    private bool[] allAttackTouched;
    private Coroutine ultimateCoroutine;
    public virtual void OnEnable()
    {
        CanAttack = true;
        canChargedAttack = false;
        numberOfAttacksInCombo = comboAttacks.Length;
    }

  
    protected virtual void OnAttack(InputValue _input)
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

    public virtual void Ultimate()
    {
        
    }
    protected void AttackIsFinished(bool touchedEnemy)
    {
        if(currentAttack == null)return;
        if (currentCombo == 0)
        {
            StartCombo();
        }
        allAttackTouched[currentCombo] = touchedEnemy;
        if (this.gameObject.activeInHierarchy)
        {
            comboCoroutine = StartCoroutine(ComboCoroutine());
            
        }
        currentAttack.Finished -= AttackIsFinished;
        CanAttack = true;
       
        currentAttack = null;
    }

    protected bool CheckIfAllTouched()
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

    protected void StartCombo()
    {
        currentCombo = 0;
        allAttackTouched = new bool[numberOfAttacksInCombo];
        for (var i = 0; i < allAttackTouched.Length; i++)
        {
            allAttackTouched[i] = false;
        }
    }

    protected IEnumerator ComboCoroutine()
    {
        currentCombo++;
        if (currentCombo >= comboAttacks.Length)
        {
            currentCombo = 0;
            if (CheckIfAllTouched())
            {
                if (ultimateCoroutine != null)
                {
                    StopCoroutine(ultimateCoroutine);
                    ultimateCoroutine = null;
                }
                ultimateCoroutine = StartCoroutine(ForUltimateComboCoroutine());
            }
        }
        yield return new WaitForSeconds(timeForDoCombo);
        currentCombo = 0;
     
    }

    protected virtual IEnumerator ForUltimateComboCoroutine()
    {
        Debug.Log("you success the combo");
        formSwitcher.CanDoUltimate = true;
        yield return new WaitForSeconds(timeForDoUltimate);
        formSwitcher.CanDoUltimate = false;
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