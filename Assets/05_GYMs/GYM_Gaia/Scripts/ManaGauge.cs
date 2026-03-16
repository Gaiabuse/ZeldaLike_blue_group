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
    [SerializeField] private int numberOfDivision;
    [SerializeField][Range(0,100)] private float pourcentageForCanSwitch;
    public bool NeedRecharge{get; private set;}
    private float currentMana;
    private Coroutine RechargeCoroutine;
    private Coroutine DecreaseCoroutine;
    private bool isPaused;
    private float currentMaxMana;
    private int currentDivision;
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
        currentMana = maxMana;
        currentMaxMana = maxMana;
        currentDivision = numberOfDivision;
        UpdateVisuals();
        RechargeCoroutine = StartCoroutine(Recharge());
    }

    private IEnumerator Recharge()
    {
        while (currentMana < currentMaxMana)
        {
            yield return new WaitUntil(() => !isPaused); 
            
            currentMana = Mathf.MoveTowards(currentMana, currentMaxMana, speedRecharge * Time.deltaTime);
            UpdateVisuals();
            float percentage = currentMana / currentMaxMana * 100f;

            if (percentage >= pourcentageForCanSwitch)
            {
                NeedRecharge = false;
            }
            yield return null;
        }
        RechargeCoroutine = null;
    }

    public void DecreaseDivision()
    {
        if(currentDivision <=1)return;
        currentDivision--;
        currentMaxMana = maxMana/numberOfDivision * currentDivision;
        if (currentMana >= currentMaxMana)
        {
            currentMana = currentMaxMana;
            UpdateVisuals();
        }
        Debug.Log(currentMaxMana);
    }

    public void IncreaseDivision()
    {
        if(currentDivision >=numberOfDivision)return;
        currentDivision++;
        float oneDivisionValue = maxMana / numberOfDivision;
        currentMaxMana =oneDivisionValue * currentDivision;
        currentMana += oneDivisionValue;
        UpdateVisuals();
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
        DecreaseCoroutine = null;
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

