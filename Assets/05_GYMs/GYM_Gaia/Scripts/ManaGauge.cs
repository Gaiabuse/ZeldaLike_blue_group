using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ManaGauge : MonoBehaviour
{
    [SerializeField] private float maxMana;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private float speedRecharge;
    [SerializeField] private float speedDecrease;
    [SerializeField] private FormSwitcher formSwitcher;
    public bool NeedRecharge{get; private set;}
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
        manaSlider.value = 0;
        manaSlider.maxValue = maxMana;
        RechargeCoroutine = StartCoroutine(Recharge());
    }

    private IEnumerator Recharge()
    {
        while (manaSlider.value <= maxMana)
        {
            yield return new WaitUntil(() => !isPaused); 
            manaSlider.value =  Mathf.MoveTowards(manaSlider.value, maxMana, speedRecharge * Time.deltaTime);
            yield return null;
        }
        NeedRecharge = false;
    }

    private IEnumerator Decrease()
    {
        while (manaSlider.value >= 0)
        {
            yield return new WaitUntil(() => !isPaused); 
            manaSlider.value =  Mathf.MoveTowards(manaSlider.value, 0, speedDecrease * Time.deltaTime);
            yield return null;
        }
        formSwitcher.ForcedTransform();
        NeedRecharge = true;
    }

    public void AddMana(float amount)
    {
        isPaused = true;
        manaSlider.DOValue(manaSlider.value + amount,0.1f).SetEase(Ease.OutBounce).OnComplete((() => isPaused = false));

    }
    

    private void OnSwitchForm(Form currentForm)
    {
        if (currentForm == Form.neutral)
        {
            RechargeCoroutine = StartCoroutine(Recharge());
            if (DecreaseCoroutine != null)
            {
                StopCoroutine(DecreaseCoroutine);
                DecreaseCoroutine = null;
            }
        }
        else
        {
            DecreaseCoroutine = StartCoroutine(Decrease());
            if (RechargeCoroutine != null)
            {
                StopCoroutine(RechargeCoroutine);
                RechargeCoroutine = null;
            }
        }
    }
    
    
}
