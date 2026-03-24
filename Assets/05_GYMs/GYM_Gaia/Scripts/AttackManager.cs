using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public abstract class AttackManager : MonoBehaviour
{
    [SerializeField] protected ManaGauge manaGauge;
    
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

    private enum inputValueDirection
    {
        up,
        down,
        left,
        right,
        none
    }
    protected virtual void OnEnable()
    {
        player.CanMove = true;
        CanAttack = true;
        canChargedAttack = false;
    }


    protected virtual void OnAttack(InputValue _input)
    {
        Vector2 inputValue = _input.Get<Vector2>();
        if (inputValue.sqrMagnitude > 0) 
        {
            inputValueDirection direction = ReturnDirection(inputValue);
            switch (direction)
            {
                case inputValueDirection.up:
                    formSwitcher.ChangeForm(Form.neutral);
                    break;
                case inputValueDirection.right:
                    formSwitcher.ChangeForm(Form.nightmare);
                    break;
                case inputValueDirection.left:
                    formSwitcher.ChangeForm(Form.dream);
                    break;
                case inputValueDirection.down:
                case inputValueDirection.none:
                default:
                    break;
            }
        }
        
    }

    private inputValueDirection ReturnDirection(Vector2 _input)
    {
        if (_input == Vector2.left)
        {
            return inputValueDirection.left;
        }

        if (_input == Vector2.right)
        {
            return inputValueDirection.right;
        }

        if (_input == Vector2.up)
        {
            return inputValueDirection.up;
        }

        if (_input == Vector2.down)
        {
            return inputValueDirection.down;
        }
        return inputValueDirection.none;
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
        CanAttack = false;
        currentAttack.Finished += AttackIsFinished;
    }

    public virtual void Ultimate()
    {
        EndForUltimate?.Invoke();
        manaGauge.AddMana(ManaAddAtSuccessCombo);
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
    public Attack Attack(ManaGauge manaGauge, Transform player)
    {
        var lAttack = UnityEngine.Object.Instantiate(AttackData.attackPrefab, player);
        lAttack.SetAttack(AttackData, type, manaGauge);
        return lAttack;
    }
}
