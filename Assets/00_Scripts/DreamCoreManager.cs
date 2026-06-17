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
    [Tooltip("size of goo when HP = 0")] [Range(0, 3)] [SerializeField] private float minGooSize = 0.1f;
    [Tooltip("size of goo HP = Maximum")] [Range(0, 3)] [SerializeField] private float maxGooSize = 0.9f;
    [SerializeField] private float gooLerpDuration = 0.2f;

    [SerializeField] private Material material;
    [SerializeField] private CanvasGroup endScreen;

    // --- PHASE GATING ---
    private bool isInvincible = true;
    private float healthCap = 0f;
    public bool isBossActive = false;

    [Header("Arena Trigger")]
    [SerializeField] private StartBossFight arenaEnterTrigger;

    // FIX: Event fired the instant HP is clamped to the phase floor,
    // so the phase manager can react mid-frame without waiting for the
    // current attack coroutine to finish naturally.
    public static event Action OnPhaseCapped;

    private void OnEnable()
    {
        PlayerController.OnRespawn += CancelBossFight;
    }

    private void OnDisable()
    {
        PlayerController.OnRespawn -= CancelBossFight;
        if (material != null)
        {
            material.SetFloat("_Size_Wobbly", 0.15f);
            material.SetFloat("_Fresnel_Smooth", 0.42f);
            material.SetVector("_Speed_Move", new Vector2(1f, 0.75f));
            material.SetFloat("_Spike_Size", 0.25f);
        }
    }

    private void Start()
    {
        isInvincible = true;
        maxHP = hp;
        _tempHP = maxHP;
        UpdateGooScale(hp);
    }

    public void StartBossFight()
    {
        isBossActive = true;
        StartCoroutine(StartFight());
        Debug.Log("BossFight");
    }

    private void CancelBossFight()
    {
        if (!isBossActive) return;
        
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopBossMusic();
        }

        StopAllCoroutines();

        var phaseManager = GetComponent<BossAttackPhaseManager>();
        if (phaseManager != null)
        {
            phaseManager.StopAllCoroutines();
            phaseManager.StopAndCleanAllAttacks(disableManager: false);
        }

        isBossActive = false;
        isInvincible = true;
        healthCap = 0f;
        hp = (int)maxHP;
        _tempHP = maxHP;

        lifeBar.SetActive(false);
        hitVFX.SetActive(false);
        UpdateLifeBarVisuals();
        UpdateGooScale(hp);

        if (material != null)
        {
            material.SetFloat("_Size_Wobbly", 0.15f);
            material.SetFloat("_Fresnel_Smooth", 0.42f);
            material.SetVector("_Speed_Move", new Vector2(1f, 0.75f));
            material.SetFloat("_Spike_Size", 0.25f);
        }

        sphereCollider.enabled = false;
        arenaObjects.SetActive(false);

        if (arenaEnterTrigger != null)
            arenaEnterTrigger.gameObject.SetActive(true);
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
        if (isInvincible || hp <= 0) return;

        float targetHP = hp - damage;

        // FIX: When a hit crosses the phase floor, clamp to the floor,
        // turn on invincibility, then fire OnPhaseCapped so the phase
        // manager can abort pending launches and start the transition
        // animation immediately — without waiting for the current
        // attack coroutine to reach its natural end.
        if (targetHP <= healthCap)
        {
            targetHP = healthCap;
            isInvincible = true;
            OnPhaseCapped?.Invoke();
        }

        targetHP = (float)Math.Round((decimal)targetHP, 2);
        hp = Mathf.Max(0, (int)targetHP);

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
        MusicManager.Instance.PlayCoreRoar();
        if (material != null)
        {
            float introDuration = 0.5f;
            float holdDuration = 0.10f;
            float outroDuration = 0.75f;

            material.SetVector("_Speed_Move", new Vector2(-1f, -3f));

            Sequence angerSequence = DOTween.Sequence();

            RumbleManager.Instance.TriggerVibration(0.5f, 0.5f);
            Tween introTween = DOVirtual.Float(0f, 1f, introDuration, value =>
            {
                material.SetFloat("_Size_Wobbly", Mathf.Lerp(0.15f, 0.35f, value));
                material.SetFloat("_Spike_Size", Mathf.Lerp(0.25f, 1f, value));
                material.SetFloat("_Fresnel_Smooth", Mathf.Lerp(2.07f, 0f, value));
            }).SetEase(Ease.OutBack);

            Tween jitterTween = DOVirtual.Float(0f, 1f, holdDuration, value =>
            {
                float jitter = Random.Range(-0.1f, 0.1f);
                RumbleManager.Instance.TriggerVibration(0.5f + jitter * 2, 0.5f + jitter * 2);
                material.SetFloat("_Size_Wobbly", 0.35f + jitter);
                material.SetFloat("_Fresnel_Smooth", 0.1f + jitter);
                material.SetFloat("_Spike_Size", 1f + jitter*2);
            }).SetLoops(5, LoopType.Yoyo);

            Tween outroTween = DOVirtual.Float(0f, 1f, outroDuration, value =>
            {
                material.SetFloat("_Size_Wobbly", Mathf.Lerp(0.35f, 0.15f, value));
                material.SetFloat("_Fresnel_Smooth", Mathf.Lerp(0f, 0.42f, value));
                material.SetFloat("_Spike_Size", Mathf.Lerp(1f, 0.25f, value));
            }).SetEase(Ease.OutBounce);

            angerSequence.Append(introTween);
            angerSequence.Append(jitterTween);
            angerSequence.Append(outroTween);

            yield return angerSequence.WaitForCompletion();

            material.SetVector("_Speed_Move", new Vector2(1f, 0.75f));
            RumbleManager.Instance.StopVibration();
        }

        Player.CanMove = true;
        Player.CanRotate = true;
        Debug.Log("Animation Finished");
    }

    public void KillBoss()
    {
        GameManager.Instance.TriggerSlowMotion();
        endScreen.DOFade(1f, 1f).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(() =>
        {
            SteamAchievements.Instance.UnlockEndGame();
        });
    }
}