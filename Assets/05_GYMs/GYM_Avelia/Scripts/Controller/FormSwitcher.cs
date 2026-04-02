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

    public void ChangeForm(Form nextForm)
    {

        if(currentForm == nextForm)return;
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
                playerController.currentAnimator = FormAttackManagers[0].FormAnimator;
                break;
            case Form.dream:
                dreamFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[1].Ultimate();
                    CanDoUltimate = false;
                }
                playerController.currentAttackManager = FormAttackManagers[1];
                playerController.currentAnimator = FormAttackManagers[1].FormAnimator;
                break;
            case Form.nightmare:
                nightmareFormObject.SetActive(true);
                if (CanDoUltimate)
                {
                    FormAttackManagers[2].Ultimate();
                    CanDoUltimate = false;
                }
                playerController.currentAttackManager = FormAttackManagers[2];
                playerController.currentAnimator = FormAttackManagers[2].FormAnimator;
                break;
        }

        currentForm = nextForm;
        SwitchForm?.Invoke(currentForm);
        playerController.CanMove = true;
        playerController.CanRotate = true;
    }
    

    public void ForcedTransform()
    {
        lastForm = currentForm;
        ChangeForm(Form.neutral);
    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}
