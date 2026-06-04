using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DreamCoreManager : MonoBehaviour
{
    [Header("Testing Tool (Play Mode Only)")]
    [SerializeField] private bool triggerSwitchPhaseTest;
    [SerializeField] private float testWaitTime = 3f;

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

    private void Start()
    {
        maxHP = hp;
        _tempHP = maxHP;

        UpdateGooScale(hp);
    }
    

    // OnValidate safely catches the inspector click and flags it for the next frame
    private void OnValidate()
    {
        if (triggerSwitchPhaseTest)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("DreamCoreManager: You must be in PLAY MODE to test the Switch Phase animation!");
                triggerSwitchPhaseTest = false;
                return;
            }
        }
    }

    private void Update()
    {
        // Safely executes the coroutine inside Unity's main loop during Play Mode
        if (triggerSwitchPhaseTest)
        {
            triggerSwitchPhaseTest = false; // Immediately reset the checkbox
            SwitchPhase(testWaitTime);
        }
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

    public void TakeDamages(int damage)
    {
        if (hp > 0)
        {
            float targetHP = (float)Math.Round((decimal)(hp - damage), 2);
            hp -= damage;

            hitVFX.transform.SetParent(transform.parent);
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y,
                Player.transform.position.z);
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

    public void SwitchPhase(float waitTimeAfterPhase)
    {
        Player.CanMove = false; // Fixed duplicate line
        Player.CanRotate = false; // Fixed duplicate line
        StartCoroutine(SwitchPhaseCoroutine(waitTimeAfterPhase));
    }
    
    private IEnumerator SwitchPhaseCoroutine(float waitTimeAfterPhase)
    {
        Debug.Log("Animation Started");
        if (material != null)
        {
            float introDuration = 0.5f;
            float holdDuration = 0.10f;
            float outroDuration = 0.75f;
    
            // CRANK up the noise speed during anger to make the shader texture go wild
            material.SetFloat("__Noise_speed", -25f); 
            
            Sequence angerSequence = DOTween.Sequence();
            
            // --- INTRO (The Explosive Outburst) ---
            // Changed to OutBack so it "overshoots" and pops out aggressively
            Tween introTween = DOVirtual.Float(0f, 1f, introDuration, value =>
            {
                material.SetFloat("_Noise_height", Mathf.Lerp(0.2f, 0.85f, value)); // Made peak height higher
                material.SetFloat("_Base_Strength", Mathf.Lerp(2.81f, -1.0f, value));
            }).SetEase(Ease.OutBack);
            
            // --- JITTER / SHAKE (The Boiling Point) ---
            // While holding, we rapidly jitter the values to look like vibrating rage
            Tween jitterTween = DOVirtual.Float(0f, 1f, holdDuration, value =>
            {
                // Random.Range creates that unstable, glitchy, erratic movement
                float jitter = Random.Range(-0.1f, 0.1f); 
                material.SetFloat("_Noise_height", 0.85f + jitter);
            }).SetLoops(5, LoopType.Yoyo); // Rapidly flips back and forth
            
            // --- OUTRO (The Cool Down) ---
            // Changed to OutBounce so it visibly bounces as it settles back to normal
            Tween outroTween = DOVirtual.Float(0f, 1f, outroDuration, value =>
            {
                material.SetFloat("_Noise_height", Mathf.Lerp(0.85f, 0.2f, value));
                material.SetFloat("_Base_Strength", Mathf.Lerp(-1.0f, 2.81f, value));
            }).SetEase(Ease.OutBounce); 
            
            // Assemble the chaos
            angerSequence.Append(introTween);
            angerSequence.Append(jitterTween); // Replaced the boring flat interval with active shaking
            angerSequence.Append(outroTween);
    
            yield return angerSequence.WaitForCompletion();
            
            // Reset to normal idle speed
            material.SetFloat("__Noise_speed", 0.23f);
            
            // Adjusted wait time calculation to include the jitter loops duration
            float totalTweenTime = introDuration + (holdDuration * 5) + outroDuration;
            yield return new WaitForSeconds(Mathf.Max(0, waitTimeAfterPhase - totalTweenTime));
        }
        else
        {
            yield return new WaitForSeconds(waitTimeAfterPhase);
        }
        
        Player.CanMove = true;
        Player.CanRotate = true;
        Debug.Log("Animation Finished");
    }
}