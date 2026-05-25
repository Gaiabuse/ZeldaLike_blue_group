using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

//[RequireComponent(typeof(Animator))]
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
    private Coroutine ultimateCoroutine;
    public static Action CanUltimate;
    public static Action EndForUltimate;
    protected bool switchInProgress =false;
    protected Coroutine finishSwitchCoroutine;
    protected bool isInUltMod = false;

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
                Debug.Log("activeControl: " + direction);
                HandleDirectionalInput(direction);
            }
            else
            {
                Debug.Log("activeControl est NULL");
            }
        }
    }
    
    protected void HandleDirectionalInput(string direction)
    {
        if(!formSwitcher.canSwitchForm)return;
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
    
        
        if (currentAttack != null)
        {
            currentAttack.Finished -= Combo;
            currentAttack.FinishedAttackFull -= FinishAttack;
            Destroy(currentAttack.gameObject); 
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }

        currentAttack = attack.Attack(player.transform);
        CanAttack = false;
        currentAttack.Finished += Combo;
        currentAttack.FinishedAttackFull += FinishAttack;
    }

    public virtual void Ultimate()
    {
        EndForUltimate?.Invoke();
    }
    protected void Combo()
    {
        CanAttack = true;
        if (currentAttack == null) return;
        if (currentCombo == 0)
        {
            StartCombo();
        }
        if (gameObject.activeInHierarchy)
        {
            comboCoroutine = StartCoroutine(ComboCoroutine());
        }
        currentAttack.Finished -= Combo;
    }

    protected void FinishAttack()
    {
        Debug.Log("finished1");
        
        player.CanMove = true;
        player.CanRotate = true;

        if (currentAttack == null) return;
        Debug.Log("finished2");
    
        currentAttack.FinishedAttackFull -= FinishAttack;
        currentAttack = null;
    }
    
    protected void StartCombo()
    {
        currentCombo = 0;
    }

    protected IEnumerator ComboCoroutine()
    {
    
        currentCombo++;
        if (currentCombo >= numberOfAttacksInCombo)
        {
            currentCombo = 0;
            FormAnimator.SetBool("isAttacking",false);
            if(isInUltMod)yield break;
            if (ultimateCoroutine != null)
            {
                StopCoroutine(ultimateCoroutine);
                ultimateCoroutine = null;
            }
            ultimateCoroutine = StartCoroutine(ForUltimateComboCoroutine());
            
        }
        yield return new WaitForSeconds(timeForDoCombo);
        FormAnimator.SetBool("isAttacking",false);
        currentCombo = 0;

    }

    protected virtual IEnumerator ForUltimateComboCoroutine()
    {
        var requiredForms = new[] { Form.neutral, Form.nightmare, Form.dream };
        if (requiredForms.All(f => formSwitcher.AvailableForms.Contains(f)))
        {
            CanUltimate?.Invoke();
            formSwitcher.CanDoUltimate = true;
        
            
            formSwitcher.NotifyUltimateReady();
            
            yield return new WaitForSecondsRealtime(formSwitcher.TimeForDoUltimate);
            
            if (formSwitcher.CanDoUltimate)
            {
                EndForUltimate?.Invoke();
                formSwitcher.EndFirstUltimateTime.Invoke();
                formSwitcher.CanDoUltimate = false;
            }
        }
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
