using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections;

public class PlayerPowder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxPowder = 50;
    [SerializeField] private float powder;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float healRate = 10f;
    [SerializeField] private float chargingTime = 1.5f;

    [Header("References")]
    [SerializeField] private Image powderBar;
    [SerializeField] private PlayerHP _hp;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private bool isTutoActionDone = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;

    private bool isHealing = false;
    private float currentChargeTimer = 0f;
    private bool wasHealingLastFrame = false; 
    private Coroutine mapSwitchCoroutine;

    private void Update()
    {
        float targetFill = powder / maxPowder;
        powderBar.fillAmount = Mathf.Lerp(powderBar.fillAmount, targetFill, Time.deltaTime * lerpSpeed);
    
        if (isHealing && powder > 0 && _hp.HP < _hp.maxHP)
        {
            currentChargeTimer += Time.deltaTime;
            wasHealingLastFrame = true; 

            if (currentChargeTimer >= chargingTime)
            {
                DoHeal();
            }
            else
            {
                RumbleManager.Instance.TriggerVibration(0.05f, 0.05f);
            }
        }
        else
        {
            if (wasHealingLastFrame)
            {
                RumbleManager.Instance.StopVibration();
                wasHealingLastFrame = false;
            }

            // Safe fallback if controls somehow get mismatched outside of callbacks
            if (!isHealing && _playerInput != null && _playerInput.actions.FindActionMap("HealMap").enabled)
            {
                InterruptAndReset();
            }
        }
    }

    private void DoHeal()
    {
        if (!isTutoActionDone)
        {
            if (tutoIndicator == null) return;
            tutoIndicator.StopBlink();
        }
        float amountToHeal = (float)Math.Round(healRate * Time.deltaTime, 2);

        if (amountToHeal > powder)
        {
            amountToHeal = powder;  
        }
        
        powder = (float)Math.Round(powder - amountToHeal, 2);
        _hp.Heal(amountToHeal);

        RumbleManager.Instance.TriggerVibration(0.5f, 0.5f);

        if (powder <= 0 || _hp.HP >= _hp.maxHP)
        {
            InterruptAndReset();
        }
    }

    public void OnHeal(InputValue value)
    {
        isHealing = value.isPressed;

        if (isHealing)
        {
            if (powder > 0 && _hp.HP < _hp.maxHP)
            {
                // Switch maps SAFELY at the end of the frame
                SafeSwitchActionMap("PlayerControl", "HealMap");
            }
            else
            {
                InterruptAndReset(); 
            }
        }
        else
        {
            InterruptAndReset();
        }
    }

    private void InterruptAndReset()
    {
        isHealing = false;
        currentChargeTimer = 0f;
        
        if (_playerInput != null)
        {
            SafeSwitchActionMap("HealMap", "PlayerControl");
        }
    }

    // Helper method to delay the switching until the Input System is done processing
    private void SafeSwitchActionMap(string mapToDisable, string mapToEnable)
    {
        if (mapSwitchCoroutine != null) StopCoroutine(mapSwitchCoroutine);
        mapSwitchCoroutine = StartCoroutine(SwitchMapsRoutine(mapToDisable, mapToEnable));
    }

    private IEnumerator SwitchMapsRoutine(string mapToDisable, string mapToEnable)
    {
        // Wait until the end of the current frame so the input system finishes its update loop
        yield return new WaitForEndOfFrame();

        if (_playerInput != null)
        {
            var disableMap = _playerInput.actions.FindActionMap(mapToDisable);
            var enableMap = _playerInput.actions.FindActionMap(mapToEnable);

            if (disableMap != null && disableMap.enabled) disableMap.Disable();
            if (enableMap != null && !enableMap.enabled) enableMap.Enable();
        }
    }

    public void GainPowder(float value)
    {
        powder = (float)Math.Round(Mathf.Clamp(powder + value, 0, maxPowder), 2);
    }
}