using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public partial class ManaGauge : MonoBehaviour
{
    [SerializeField] private float maxMana;
    [SerializeField] private Image manaSlider;
    [SerializeField] private float speedRecharge;
    [SerializeField] private float speedDecrease;
    [SerializeField] private FormSwitcher formSwitcher;
    public bool NeedRecharge{get; private set;}
    private float currentMana;
    private Coroutine RechargeCoroutine;
    private Coroutine DecreaseCoroutine;
    private bool isPaused;

    private void OnEnable()
    {
        FormSwitcher.SwitchForm += OnSwitchForm;
    }

    private void OnDisable()
    {
        FormSwitcher.SwitchForm -= OnSwitchForm;
    }

    private void Start()
    {
        currentMana = 0;
        UpdateVisuals();
        RechargeCoroutine = StartCoroutine(Recharge());
    }

    private IEnumerator Recharge()
    {

        while (currentMana < maxMana)
        {
            yield return new WaitUntil(() => !isPaused); 
            
            currentMana = Mathf.MoveTowards(currentMana, maxMana, speedRecharge * Time.deltaTime);
            UpdateVisuals();
            
            yield return null;
        }
        NeedRecharge = false;
    }

    private IEnumerator Decrease()
    {
        while (currentMana > 0)
        {
            yield return new WaitUntil(() => !isPaused); 
            currentMana = Mathf.MoveTowards(currentMana, 0, speedDecrease * Time.deltaTime);
            UpdateVisuals();
        }
        formSwitcher.ForcedTransform();
        NeedRecharge = true;
    }

    public void AddMana(float amount)
    {
        isPaused = true;
        float targetMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        
        manaSlider.DOFillAmount(NormalizeValue(targetMana), 0.1f)
            .SetEase(Ease.OutBounce)
            .OnUpdate(() => {
                currentMana = manaSlider.fillAmount * maxMana;
            })
            .OnComplete(() => isPaused = false);
    }
    

    private void OnSwitchForm(Form currentForm)
    {
        if (currentForm == Form.neutral)
        {
            if (RechargeCoroutine != null)
            {
                StopCoroutine(RechargeCoroutine);
                RechargeCoroutine = null;
            }
            RechargeCoroutine = StartCoroutine(Recharge());
            if (DecreaseCoroutine != null)
            {
                StopCoroutine(DecreaseCoroutine);
                DecreaseCoroutine = null;
            }
        }
        else
        {
            if (DecreaseCoroutine != null)
            {
                StopCoroutine(DecreaseCoroutine);
                DecreaseCoroutine = null;
            }
            DecreaseCoroutine = StartCoroutine(Decrease());
            if (RechargeCoroutine != null)
            {
                StopCoroutine(RechargeCoroutine);
                RechargeCoroutine = null;
            }
        }
    }
    private void UpdateVisuals()
    {
        manaSlider.fillAmount = NormalizeValue(currentMana);
    }

    private float NormalizeValue(float value)
    {
        return Mathf.Clamp01(value / maxMana);
    }
    
}

