using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public abstract class AttackManager : MonoBehaviour
{
    
    public Animator FormAnimator;
    [SerializeField] protected float timeForDoCombo;
    [SerializeField] protected PlayerController player;
    [SerializeField] private int ManaAddAtSuccessCombo = 5;
    [SerializeField] protected FormSwitcher formSwitcher;
    [HideInInspector]public bool CanAttack;
    protected bool canChargedAttack;
    protected Attack currentAttack;
    protected int currentCombo;
    protected Coroutine comboCoroutine;
    protected int numberOfAttacksInCombo;
    private bool[] allAttackTouched;
    private Coroutine ultimateCoroutine;
    public static Action CanUltimate;
    public static Action EndForUltimate;
    protected bool switchInProgress =false;
    protected Coroutine finishSwitchCoroutine;
    protected virtual void OnEnable()
    {
        player.CanMove = true;
        CanAttack = true;
        canChargedAttack = false;
        switchInProgress = true;
        currentCombo = 0;
    }

    protected IEnumerator FinishSwitch()
    {
        yield return new WaitForSeconds(0.1f);
        switchInProgress = false;
    }
    protected virtual void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            var action = player.playerInput.actions["Attack"];
        
            if (action.activeControl != null)
            {
                string direction = action.activeControl.name; 
                
                HandleDirectionalInput(direction);
            }
        }
       
    }
    
    private void HandleDirectionalInput(string direction)
    {
        switch (direction)
        {
            case "buttonNorth":
                if (formSwitcher.currentForm != Form.neutral)
                {
                    formSwitcher.ChangeForm(Form.neutral);
                }
                break;
            case "buttonEast" :
                if (formSwitcher.currentForm != Form.nightmare)
                {
                    formSwitcher.ChangeForm(Form.nightmare);
                }
                
                break;
            case "buttonWest":
                if (formSwitcher.currentForm != Form.dream)
                {
                    formSwitcher.ChangeForm(Form.dream);
                }
                break;
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
        currentAttack = attack.Attack(player.transform);
        CanAttack = false;
        currentAttack.Finished += AttackIsFinished;
    }

    public virtual void Ultimate()
    {
        EndForUltimate?.Invoke();
    }
    protected void AttackIsFinished(bool touchedEnemy)
    {
        if (currentAttack == null) return;
        player.CanMove = true;
        player.CanRotate = true;
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
        if (currentCombo >= numberOfAttacksInCombo)
        {
            currentCombo = 0;
            if (CheckIfAllTouched())
            {
                Debug.Log("canUltimate");
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
        CanUltimate?.Invoke();
        formSwitcher.CanDoUltimate = true;
        yield return new WaitForSeconds(formSwitcher.TimeForDoUltimate);
        EndForUltimate?.Invoke();
        formSwitcher.CanDoUltimate = false;
    }
}

[Serializable]
public class SimpleAttack
{
    [SerializeField] private AttackData AttackData;
    [SerializeField] private Attack.TypeOfAttack type;
    public Attack Attack(Transform player)
    {
        var lAttack = UnityEngine.Object.Instantiate(AttackData.attackPrefab, player);
        lAttack.SetAttack(AttackData, type);
        return lAttack;
    }
}
