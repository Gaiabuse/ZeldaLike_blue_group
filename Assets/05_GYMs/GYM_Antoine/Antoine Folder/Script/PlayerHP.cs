using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PlayerHP : MonoBehaviour, IPlayerDamageable
{
    public bool invicible;
    [SerializeField] public int maxHP = 100;
    [SerializeField] public int startHP = 70;
    [SerializeField] private PlayerController playerController;
    public Action OnTakeDamage;
    [SerializeField] private float speedRecharge;

    [Header("Settings Visuals")]
    [Tooltip("value when HP = 0")]
    [Range(0, 1)][SerializeField] private float minFillAmount = 0.1f;
    [Tooltip("value when HP = Maximum")]
    [Range(0, 1)][SerializeField] private float maxFillAmount = 0.9f;
    [SerializeField] Image healthBar;
    [SerializeField] Image damagesBar;
    [SerializeField] VisualEffect healVFX;
    [SerializeField] CanvasGroup deathScreen;
    
    [Header("LowLife Feedback")]
    [SerializeField] private Volume lowLifeVolume;
    [SerializeField] private float lifeThreshold = 15f;
    [SerializeField] private float blinkTime = 0.5f;
    [SerializeField] private Vector2 volumeDeltaIntensity;
    private Vignette vignette;
    private Tween lowLifeTween;
    private Tween lowLifeFadeInTween;
    private Tween lowLifeFadeOutTween;

    private Coroutine damageCoroutine;
    private Coroutine healCoroutine;
    public float HP;
    [SerializeField] private float tempHP;

    private void Start()
    {
        // ONLY runs once when the game boots up
        HP = startHP;
        tempHP = startHP;
        UpdateVisuals();
        
        if (lowLifeVolume.profile.TryGet(out Vignette vig))
        {
            vignette = vig;
        }
    }

    public void TakeDamage(int damage, float stun = 0f)
    {
        if (invicible) return;
        if (HP > 0)
        {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            if (healCoroutine != null) StopCoroutine(healCoroutine);

            HP -= damage;
            Camera.main.transform.DOShakePosition(0.5f, 0.5f);
            
            if (HP <= 0)
            {
                HP = 0;
                HandleDeath();
                return;
            }
            
            if (HP <= lifeThreshold)
            {
                LowLifeFeedback();
            }
            else
            {
                StopLowLifeFeedback();
            }

            float targetHP = (float)Math.Round(HP, 2);
            damageCoroutine = StartCoroutine(VisualDamage(targetHP));
        }

        OnTakeDamage?.Invoke();
    }

    private void LowLifeFeedback()
    {
        if (lowLifeTween != null && lowLifeTween.IsActive() && lowLifeTween.IsPlaying()) return;

        lowLifeFadeOutTween?.Kill();
        lowLifeFadeInTween?.Kill();
        lowLifeTween?.Kill();

        lowLifeFadeInTween = DOTween.To(() => vignette.intensity.value,
            x => vignette.intensity.value = x,
            volumeDeltaIntensity.x,
            blinkTime * 2).OnComplete(() =>
        {
            lowLifeTween = DOTween.To(() => vignette.intensity.value,
                x => vignette.intensity.value = x,
                volumeDeltaIntensity.y,
                blinkTime).SetLoops(-1, LoopType.Yoyo);
        });
    }

    private void StopLowLifeFeedback()
    {
        lowLifeFadeInTween?.Kill();
        lowLifeTween?.Kill();
        lowLifeFadeInTween = null;
        lowLifeTween = null;

        lowLifeFadeOutTween = DOTween.To(() => vignette.intensity.value,
            x => vignette.intensity.value = x,
            0,
            blinkTime * 4);
    }

    private void HandleDeath()
    {
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        tempHP = 0;
        HP = 0;

        UpdateVisuals();
        deathScreen.DOFade(1f, 0.5f).OnComplete(() =>
        {
            playerController.TriggerRespawn();
            deathScreen.DOFade(0f, 0.5f);
        });
    }

    public void ResetHealth()
    {
        if (damageCoroutine != null) StopCoroutine(damageCoroutine);
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        StopLowLifeFeedback();
        HP = maxHP;
        tempHP = maxHP;
        UpdateVisuals();
    }

    private void StopHealing()
    {
        if (healVFX != null) healVFX.enabled = false;
        if (healCoroutine != null) StopCoroutine(healCoroutine);
    }

    private void HealAtMax()
    {
        Heal(maxHP - HP);
    }

    public void Heal(float heal)
    {
        if (HP >= maxHP) return;

        HP = (float)Math.Round(Mathf.Min(HP + heal, maxHP), 2);
        tempHP = HP;

        if (HP > lifeThreshold)
            StopLowLifeFeedback();

        if (healVFX != null) healVFX.enabled = true;
        if (healCoroutine != null) StopCoroutine(healCoroutine);
        healCoroutine = StartCoroutine(VisualHeal(HP));
    }

    private IEnumerator VisualHeal(float targetHP)
    {
        while (Mathf.Abs(damagesBar.fillAmount - NormalizeValue(targetHP)) > 0.001f)
        {
            float currentFill = healthBar.fillAmount;
            float targetFill = NormalizeValue(targetHP);

            healthBar.fillAmount = Mathf.MoveTowards(currentFill, targetFill, speedRecharge * Time.deltaTime);
            yield return null;
        }
        healCoroutine = null;
        if (healVFX != null) healVFX.enabled = false;
    }

    private IEnumerator VisualDamage(float newLife)
    {
        while (tempHP > newLife)
        {
            float nextHP = Mathf.MoveTowards(tempHP, newLife, speedRecharge * Time.deltaTime);
            tempHP = (float)Math.Round(nextHP, 2);

            UpdateVisuals();
            yield return null;
        }
        damageCoroutine = null;
    }

    private void UpdateVisuals()
    {
        if (healthBar != null) healthBar.fillAmount = NormalizeValue(HP);
        if (damagesBar != null) damagesBar.fillAmount = NormalizeValue(tempHP);
    }

    private float NormalizeValue(float value)
    {
        float lifeRatio = Mathf.Clamp01(value / (float)maxHP);
        return Mathf.Lerp(minFillAmount, maxFillAmount, lifeRatio);
    }
}
