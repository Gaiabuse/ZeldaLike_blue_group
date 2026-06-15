using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.VFX;

public class FormSwitcher : MonoBehaviour
{
    public Form currentForm { get; private set; } = Form.neutral;

    [SerializeField] GameObject neutralFormObject, dreamFormObject, nightmareFormObject;
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
    public GameObject tutoTrigger;
    public float TimeForDoUltimate { private set; get; }
    public List<Form> AvailableForms;

    public bool CanDoUltimate;

    private bool isFirstUltimateTime = false;
    public bool canSwitchForm = true;
    public Action FirstUltimateTime;
    public Action EndFirstUltimateTime;

    private Coroutine ultTimerCoroutine;

    private void Start()
    {
        CanDoUltimate = false;
        canSwitchForm = true;
        isFirstUltimateTime = true;
        TimeForDoUltimate = timeForDoUltimate;
    }

    private void NotifyUltimateReady()
    {
        if (isFirstUltimateTime)
        {
            FirstUltimateTime?.Invoke();
            isFirstUltimateTime = false;
        }
    }

    public void StartUltimateWindow()
    {
        if (ultTimerCoroutine != null)
        {
            StopCoroutine(ultTimerCoroutine);
        }
        ultTimerCoroutine = StartCoroutine(UltimateWindowCoroutine());
    }

    private IEnumerator UltimateWindowCoroutine()
    {
        var requiredForms = new[] { Form.neutral, Form.nightmare, Form.dream };
        if (requiredForms.All(f => AvailableForms.Contains(f)))
        {
            bool wasFirstTime = isFirstUltimateTime;
            AttackManager.CanUltimate?.Invoke();
            CanDoUltimate = true;
            NotifyUltimateReady(); // fires FirstUltimateTime, flips isFirstUltimateTime to false

            if (wasFirstTime)
            {
                tutoTrigger.SetActive(true);
                yield return new WaitUntil(() => !CanDoUltimate);
                // At this point ChangeForm() already handled EndFirstUltimateTime + reset
            }
            else
            {
                yield return new WaitForSecondsRealtime(TimeForDoUltimate);

                if (CanDoUltimate)
                {
                    AttackManager.EndForUltimate?.Invoke();
                    EndFirstUltimateTime?.Invoke();
                    CanDoUltimate = false;
                }
            }
        }
    }

    public void ChangeForm(Form nextForm)
    {
        if (!AvailableForms.Contains(nextForm)) return;
        if (currentForm == nextForm) return;

        bool wasCanDoUltimate = CanDoUltimate; 
        Debug.Log(wasCanDoUltimate);
        
        // Note: If you want changing forms to CANCEL the ultimate, leave the lines below.
        // If you want to keep the ultimate active across forms, comment out this 'if' block.
        if (CanDoUltimate)
        {
            EndFirstUltimateTime?.Invoke();
            CanDoUltimate = false;
            if (ultTimerCoroutine != null)
            {
                StopCoroutine(ultTimerCoroutine);
            }
        }

        neutralFormObject.SetActive(false);
        dreamFormObject.SetActive(false);
        nightmareFormObject.SetActive(false);

        switch (nextForm)
        {
            case Form.neutral:
                switchToNeutralFX.Play();
                neutralFormObject.SetActive(true);
                if (wasCanDoUltimate) FormAttackManagers[0].Ultimate();
                else if (ultIndicatorSprites.Count > 0) ultIndicator.sprite = ultIndicatorSprites[0];
                playerController.currentAttackManager = FormAttackManagers[0];
                playerController.currentAnimator = FormAttackManagers[0].FormAnimator;
                break;

            case Form.dream:
                switchToDreamFX.Play();
                dreamFormObject.SetActive(true);
                if (wasCanDoUltimate) FormAttackManagers[1].Ultimate();
                else if (ultIndicatorSprites.Count > 1) ultIndicator.sprite = ultIndicatorSprites[1];
                playerController.currentAttackManager = FormAttackManagers[1];
                playerController.currentAnimator = FormAttackManagers[1].FormAnimator;
                break;

            case Form.nightmare:
                switchToNightmareFX.Play();
                nightmareFormObject.SetActive(true);
                if (wasCanDoUltimate) FormAttackManagers[2].Ultimate();
                else if (ultIndicatorSprites.Count > 2) ultIndicator.sprite = ultIndicatorSprites[2];
                playerController.currentAttackManager = FormAttackManagers[2];
                playerController.currentAnimator = FormAttackManagers[2].FormAnimator;
                break;
        }
        
        if (MusicManager.Instance != null) MusicManager.Instance.PlaySwitchForm();
        currentForm = nextForm;
        SwitchForm?.Invoke(currentForm);
        playerController.CanMove = true;
        playerController.CanRotate = true;
    }

    public void ForcedTransform()
    {
        if (AvailableForms.Count > 0) ChangeForm(AvailableForms[0]);
    }
}

public enum Form
{
    neutral,
    dream,
    nightmare,
}