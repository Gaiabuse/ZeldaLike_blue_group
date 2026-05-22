using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class FormSwitcher : MonoBehaviour
{
    public Form currentForm { get; private set; } = Form.neutral;

    [SerializeField]
    GameObject neutralFormObject, dreamFormObject, nightmareFormObject;
    public static Action<Form> SwitchForm;
    private Form lastForm = Form.neutral;
    
    [SerializeField] private VisualEffect switchToNeutralFX;
    [SerializeField] private VisualEffect switchToDreamFX;
    [SerializeField] private VisualEffect switchToNightmareFX;
    
    [SerializeField] private Image ultIndicator;
    [SerializeField] private List<Sprite> ultIndicatorSprites;

    [SerializeField] private AttackManager[] FormAttackManagers;

    [SerializeField] PlayerController playerController;
    [SerializeField] private float timeForDoUltimate;
    public float TimeForDoUltimate{private set; get;}
    public List<Form> AvailableForms;

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

        if (!AvailableForms.Contains(nextForm)) return;
        if(currentForm == nextForm)return;
        neutralFormObject.SetActive(false);
        dreamFormObject.SetActive(false);
        nightmareFormObject.SetActive(false);
        switch (nextForm)
        {
            case Form.neutral:
                switchToNeutralFX.Play();
                neutralFormObject.SetActive(true);
                ultIndicator.sprite = ultIndicatorSprites[0];
                if (CanDoUltimate)
                {
                    FormAttackManagers[0].Ultimate();
                    CanDoUltimate = false;
                }

                playerController.currentAttackManager = FormAttackManagers[0];
                playerController.currentAnimator = FormAttackManagers[0].FormAnimator;
                break;
            case Form.dream:
                switchToDreamFX.Play();
                dreamFormObject.SetActive(true);
                ultIndicator.sprite = ultIndicatorSprites[1];
                if (CanDoUltimate)
                {
                    FormAttackManagers[1].Ultimate();
                    CanDoUltimate = false;
                }
                playerController.currentAttackManager = FormAttackManagers[1];
                playerController.currentAnimator = FormAttackManagers[1].FormAnimator;
                break;
            case Form.nightmare:
                switchToNightmareFX.Play();
                nightmareFormObject.SetActive(true);
                ultIndicator.sprite = ultIndicatorSprites[2];
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
        ChangeForm(AvailableForms[0]);
    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}
