using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

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

    private bool isHealing = false;
    private float currentChargeTimer = 0f;

    private void Update()
    {
        float targetFill = powder / maxPowder;
        powderBar.fillAmount = Mathf.Lerp(powderBar.fillAmount, targetFill, Time.deltaTime * lerpSpeed);

        if (isHealing && powder > 0 && _hp.HP < _hp.maxHP)
        {
            currentChargeTimer += Time.deltaTime;

            if (currentChargeTimer >= chargingTime)
            {
                DoHeal();
            }
            else
            {
                if (Gamepad.current != null)
                    Gamepad.current.SetMotorSpeeds(0.05f, 0.05f);
            }
        }
    }

    private void DoHeal()
    {
        float amountToHeal = (float)Math.Round(healRate * Time.deltaTime, 2);

        if (amountToHeal > powder)
        {
            amountToHeal = powder;  
        }
        
        powder = (float)Math.Round(powder - amountToHeal, 2);
        _hp.Heal(amountToHeal);

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0.5f, 0.5f);  
        }
    }

    public void OnHeal(InputValue value)
    {
        isHealing = value.isPressed;

        if (isHealing)
        {
            _playerInput.actions.FindActionMap("PlayerControl").Disable();
        }
        else
        {
            ResetHealingState();
            StopHealEffects();
        }
    }

    private void StopHealEffects()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);

        if (!isHealing) return; 
        
        if (powder <= 0 || _hp.HP >= _hp.maxHP)
        {
            ResetHealingState();
        }
    }

    private void ResetHealingState()
    {
        currentChargeTimer = 0f;
        if (!_playerInput.actions.FindActionMap("PlayerControl").enabled)
        {
            _playerInput.actions.FindActionMap("PlayerControl").Enable();
        }
    }

    public void GainPowder(float value)
    {
        powder = (float)Math.Round(Mathf.Clamp(powder + value, 0, maxPowder), 2);
    }
}