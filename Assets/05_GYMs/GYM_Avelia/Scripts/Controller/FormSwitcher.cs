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

    private void ChangeForm(Form nextForm)
    {
        
        neutralFormObject.SetActive(false);
        dreamFormObject.SetActive(false);
        nightmareFormObject.SetActive(false);

        switch (nextForm)
        {
            case Form.neutral:
                neutralFormObject.SetActive(true);
                break;
            case Form.dream:
                dreamFormObject.SetActive(true);
                break;
            case Form.nightmare:
                nightmareFormObject.SetActive(true);
                break;
        }

        currentForm = nextForm;
    }

    void OnTransform(InputValue _input)
    {
        if (currentForm == Form.neutral || manaGauge.NeedRecharge) return;
        Debug.Log("transform");
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
        Debug.Log("switch");
        if (lastForm != Form.neutral)
        {
            switch (lastForm)
            {
                case Form.dream:
                    ChangeForm(Form.dream);
                    lastForm = Form.neutral;
                    SwitchForm?.Invoke(currentForm);
                    return;
                case Form.nightmare:
                    ChangeForm(Form.nightmare);
                    lastForm = Form.neutral;
                    SwitchForm?.Invoke(currentForm);
                    return;
            }
        }

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
                ChangeForm(Form.dream);
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
