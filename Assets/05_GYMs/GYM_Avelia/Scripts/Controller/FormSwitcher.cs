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
    [SerializeField] private ManaGauge manaGauge;

    [SerializeField] private AttackManager[] FormAttackManagers;

    [SerializeField] PlayerController playerController;
    [SerializeField] private float timeForDoUltimate;
    public float TimeForDoUltimate{private set; get;}
    public bool CanDoUltimate;

    public bool canSwitchForm = true;
    private void Start()
    {
        CanDoUltimate = false;
        canSwitchForm = true;
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

                playerController.currentAttackManager = FormAttackManagers[0];
                break;
            case Form.dream:
                dreamFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[1].Ultimate();
                    CanDoUltimate = false;
                }
                playerController.currentAttackManager = FormAttackManagers[1];
                break;
            case Form.nightmare:
                nightmareFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[2].Ultimate();
                    CanDoUltimate = false;
                }
                playerController.currentAttackManager = FormAttackManagers[2];
                break;
        }

        currentForm = nextForm;
    }

    void OnSwitchLeft(InputValue _input)
    {
        if (manaGauge.NeedRecharge || !canSwitchForm) return;
        switch (currentForm)
        {
            case Form.neutral:
                ChangeForm(Form.dream);
                break;
            case Form.dream:
                ChangeForm(Form.nightmare);
                break;
            case Form.nightmare:
                ChangeForm(Form.neutral);
                break;
        }
        SwitchForm?.Invoke(currentForm);
    }

    public void ForcedTransform()
    {
        lastForm = currentForm;
        ChangeForm(Form.neutral);
        SwitchForm?.Invoke(currentForm);
    }
    void OnSwitchRight(InputValue _input)
    {
        if (manaGauge.NeedRecharge || !canSwitchForm) return;

        switch (currentForm)
        {
            case Form.neutral:
                ChangeForm(Form.nightmare);
                break;
            case Form.dream:
                ChangeForm(Form.neutral);
                break;
            case Form.nightmare:
                ChangeForm(Form.dream);
                break;
        }
        SwitchForm?.Invoke(currentForm);
    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}
