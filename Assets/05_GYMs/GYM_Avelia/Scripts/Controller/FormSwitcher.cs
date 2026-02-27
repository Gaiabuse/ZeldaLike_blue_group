using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FormSwitcher : MonoBehaviour
{
    public Form currentForm { get; private set; } = Form.neutral;

    [SerializeField]
    GameObject neutralFormObject, dreamFormObject, nightmareFormObject;
    public static Action<Form> SwitchForm;
    private Form lastForm = Form.neutral;
    [SerializeField]private ManaGauge manaGauge;

    [SerializeField] private AttackManager[] FormAttackManagers;

    [SerializeField] private float timeForDoUltimate;
    public float TimeForDoUltimate{private set; get;}
    public bool CanDoUltimate;

    private void Start()
    {
        CanDoUltimate = false;
        TimeForDoUltimate = timeForDoUltimate;
    }

    private void ChangeForm(Form nextForm)
    {
        
        neutralFormObject.SetActive(false);
        dreamFormObject.SetActive(false);
        nightmareFormObject.SetActive(false);
        switch (nextForm)
        {
            case Form.neutral:
                neutralFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[0].Ultimate();
                    CanDoUltimate = false;
                }
                break;
            case Form.dream:
                dreamFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[1].Ultimate();
                    CanDoUltimate = false;
                }
                break;
            case Form.nightmare:
                nightmareFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[2].Ultimate();
                    CanDoUltimate = false;
                }
                break;
        }

        currentForm = nextForm;
    }

    void OnTransform(InputValue _input)
    {
        if (manaGauge.NeedRecharge) return;
        if (currentForm == Form.neutral)
        {
            lastForm = currentForm;
            ChangeForm(Form.dream);
            SwitchForm?.Invoke(currentForm);
            return;
        }
        lastForm = currentForm;
        ChangeForm(Form.neutral);
        SwitchForm?.Invoke(currentForm);
    }

    public void ForcedTransform()
    {
        lastForm = currentForm;
        ChangeForm(Form.neutral);
        SwitchForm?.Invoke(currentForm);
    }
    void OnSwitch(InputValue _input)
    {
        if(manaGauge.NeedRecharge)return;
        
        switch (currentForm)
        {
            case Form.dream:
                ChangeForm(Form.nightmare);
                SwitchForm?.Invoke(currentForm);
                return;
            case Form.nightmare:
                ChangeForm(Form.dream);
                SwitchForm?.Invoke(currentForm);
                return;
            case Form.neutral:
                ChangeForm(Form.nightmare);
                SwitchForm?.Invoke(currentForm);
                return;
        }

    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}
