using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DreamCoreManager : MonoBehaviour
{
    [SerializeField] private PlayerController Player;
    [SerializeField] private int hp = 1000;

    [Header("ArenaTriggers")] 
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private GameObject arenaObjects;
    [SerializeField] private float timeForStarFight = 0.5f;
    
    [Header("Damage Display")] 
    [SerializeField] protected GameObject hitVFX;

    [Header("Life display")] 
    [SerializeField] private GameObject lifeBar;
    [SerializeField] private Image frontLife;
    [SerializeField] private Image dmgLife;
    [SerializeField] private float bounceDuration;
    private float _tempHP;
    private float maxHP;
    [Tooltip("value when HP = 0")] [Range(0, 1)] [SerializeField] private float minFillAmount = 0.1f;
    [Tooltip("value when HP = Maximum")] [Range(0, 1)] [SerializeField] private float maxFillAmount = 0.9f;

    [Header("Goo Size Display")] 
    [Tooltip("size of goo when HP = 0")] [Range(0, 2)] [SerializeField] private float minGooSize = 0.1f;
    [Tooltip("size of goo HP = Maximum")] [Range(0, 2)] [SerializeField] private float maxGooSize = 0.9f;
    [SerializeField] private float gooLerpDuration = 0.2f;
    
    [SerializeField] private Material material;

    // --- NEW PHASE GATING VARIABLES ---
    private bool isInvincible = true;
    private float healthCap = 0f; // The absolute lowest HP the boss can reach in the current sub-phase

    private void Start()
    {
        isInvincible = true;
        maxHP = hp;
        _tempHP = maxHP;

        UpdateGooScale(hp);
    }

    public void StartBossFight()
    {
        StartCoroutine(StartFight());
    }

    private IEnumerator StartFight()
    {
        sphereCollider.enabled = true;
        arenaObjects.SetActive(true);
        
        Player.CanMove = false;
        Player.CanRotate = false;
        
        yield return new WaitForSeconds(0.25f);
        
        if (!lifeBar.activeSelf)
        {
            lifeBar.SetActive(true);
            lifeBar.GetComponent<CanvasGroup>().DOFade(1f, gooLerpDuration);
            lifeBar.transform.DOScale(1f, bounceDuration).SetEase(Ease.OutCubic);
        }
        
        yield return new WaitForSeconds(timeForStarFight);
        
        Player.CanMove = true;
        Player.CanRotate = true;
        GetComponent<BossAttackPhaseManager>().StartBossAttack();
    }

    // --- NEW PUBLIC METHODS TO CONTROL INVINCIBILITY ---
    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }

    public void SetHealthCap(float minHPThreshold)
    {
        healthCap = minHPThreshold;
    }

    public void TakeDamages(int damage)
    {
        // 1. If explicitly invincible, block damage entirely
        if (isInvincible || hp <= 0) return;

        // 2. Calculate intended new health
        float targetHP = hp - damage;

        // 3. GATEKEEPING: If this damage crosses the next phase threshold, clamp it!
        if (targetHP <= healthCap)
        {
            targetHP = healthCap;
            isInvincible = true; // Automatically turn on invincibility because we hit a wall
        }

        targetHP = (float)Math.Round((decimal)targetHP, 2);
        hp = Mathf.Max(0, (int)targetHP); // Ensure it doesn't go below 0

        // Visuals
        hitVFX.transform.SetParent(transform.parent);
        hitVFX.transform.position = transform.position;
        Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
        hitVFX.transform.LookAt(lookTarget);
        hitVFX.transform.Rotate(0, 90, 0);

        hitVFX.SetActive(false);
        hitVFX.SetActive(true);

        StartCoroutine(VisualDamage(targetHP));
        UpdateGooScale(hp);

        if (hp <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        lifeBar.SetActive(false);
        hitVFX.SetActive(false);
    
        if (TryGetComponent<BossAttackPhaseManager>(out var phaseManager))
        {
            phaseManager.StopAndCleanAllAttacks();
        }
    }

    private IEnumerator VisualDamage(float newLife)
    {
        while (_tempHP > newLife)
        {
            float nextHP = Mathf.MoveTowards(_tempHP, newLife, 50 * Time.deltaTime);
            _tempHP = (float)Math.Round(nextHP, 2);

            UpdateLifeBarVisuals();
            yield return null;
        }
    }

    private void UpdateLifeBarVisuals()
    {
        frontLife.fillAmount = NormalizeValue(hp);
        dmgLife.fillAmount = NormalizeValue(_tempHP);
    }

    private void UpdateGooScale(float currentHP)
    {
        float lifeRatio = Mathf.Clamp01(currentHP / maxHP);
        float targetGooSize = Mathf.Lerp(minGooSize, maxGooSize, lifeRatio);

        transform.DOKill();
        transform.DOScale(new Vector3(targetGooSize, targetGooSize, targetGooSize), gooLerpDuration).SetEase(Ease.OutQuad);
    }

    private float NormalizeValue(float value)
    {
        float lifeRatio = Mathf.Clamp01(value / maxHP);
        return Mathf.Lerp(minFillAmount, maxFillAmount, lifeRatio);
    }
    
    public IEnumerator SwitchPhaseCoroutine()
    {
        Player.gameObject.GetComponent<PlayerPowder>().GainPowder(25);
        Debug.Log("Animation Started");
        if (material != null)
        {
            float introDuration = 0.5f;
            float holdDuration = 0.10f;
            float outroDuration = 0.75f;
    
            material.SetFloat("__Noise_speed", -25f); 
            
            Sequence angerSequence = DOTween.Sequence();

            RumbleManager.Instance.TriggerVibration(0.5f,0.5f);
            Tween introTween = DOVirtual.Float(0f, 1f, introDuration, value =>
            {
                material.SetFloat("_Noise_height", Mathf.Lerp(0.2f, 0.85f, value)); 
                material.SetFloat("_Base_Strength", Mathf.Lerp(2.81f, -1.0f, value));
            }).SetEase(Ease.OutBack);
            
            Tween jitterTween = DOVirtual.Float(0f, 1f, holdDuration, value =>
            {
                float jitter = Random.Range(-0.1f, 0.1f); 
                RumbleManager.Instance.TriggerVibration(0.5f + jitter*2,0.5f + jitter*2);
                material.SetFloat("_Noise_height", 0.85f + jitter);
            }).SetLoops(5, LoopType.Yoyo);
            
            Tween outroTween = DOVirtual.Float(0f, 1f, outroDuration, value =>
            {
                material.SetFloat("_Noise_height", Mathf.Lerp(0.85f, 0.2f, value));
                material.SetFloat("_Base_Strength", Mathf.Lerp(-1.0f, 2.81f, value));
            }).SetEase(Ease.OutBounce); 
            
            angerSequence.Append(introTween);
            angerSequence.Append(jitterTween);
            angerSequence.Append(outroTween);
    
            yield return angerSequence.WaitForCompletion();
            
            material.SetFloat("__Noise_speed", 0.23f);
            RumbleManager.Instance.StopVibration();
        }
        
        Player.CanMove = true; Player.CanRotate = true;
        Debug.Log("Animation Finished");
    }

    private void OnDisable()
    {
        material.SetFloat("_Noise_height", 0.2f);
        material.SetFloat("_Base_Strength", 2.81f);
        material.SetFloat("__Noise_speed", 0.23f);
    }
}