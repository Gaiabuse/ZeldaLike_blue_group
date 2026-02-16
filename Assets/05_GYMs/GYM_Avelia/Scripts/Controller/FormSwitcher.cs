using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FormSwitcher : MonoBehaviour
{
    public Form currentForm { get; private set; } = Form.neutral;

    [SerializeField]
    GameObject neutralFormObject, dreamFormObject, nightmareFormObject;
    public static Action SwitchForm;
    private Form lastForm = Form.neutral;

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
        if (currentForm == Form.neutral) return;
        Debug.Log("transform");
        lastForm = currentForm;
        ChangeForm(Form.neutral);
        SwitchForm?.Invoke();
    }

    void OnSwitch(InputValue _input)
    {
        Debug.Log("switch");
        if (lastForm != Form.neutral)
        {
            switch (lastForm)
            {
                case Form.dream:
                    ChangeForm(Form.dream);
                    lastForm = Form.neutral;
                    SwitchForm?.Invoke();
                    return;
                case Form.nightmare:
                    ChangeForm(Form.nightmare);
                    lastForm = Form.neutral;
                    SwitchForm?.Invoke();
                    return;
            }
        }

        switch (currentForm)
        {
            case Form.dream:
                ChangeForm(Form.nightmare);
                SwitchForm?.Invoke();
                return;
            case Form.nightmare:
                ChangeForm(Form.dream);
                SwitchForm?.Invoke();
                return;
            case Form.neutral:
                ChangeForm(Form.dream);
                SwitchForm?.Invoke();
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
