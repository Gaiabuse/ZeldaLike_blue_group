using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class EnnemyBase : MonoBehaviour, IEnemyDamageable
{
    protected Animator animator;

    [Header("Data")]
    [SerializeField] protected EnemyData data;

    [SerializeField] protected int HP = 5;
    protected Vector2 speed;
    protected Vector2 acceleration;
    protected Vector2 SpeedRotate;

    [SerializeField] protected bool invincible = false;
    [SerializeField] protected bool showDamageDisplayInvincible = true;

    [Header("Basic")]
    [SerializeField] protected Transform Player;
    [SerializeField] protected Transform Leure;

    protected bool TargetInFieldOfView;
    protected Transform CurrentTarget;

    protected float timerGeneral = 0;

    public bool alwaysAgro;

    [SerializeField] float stunMultiplier = 1f;
    
    [SerializeField] protected string _move;

    [Header("Deal Damage")]
    [SerializeField] protected EnnemyHit MainHitBox;

    [Header("Eyes")]
    [SerializeField] protected List<MeshRenderer> Eyes;
    [SerializeField] protected Color colorNormal;
    [SerializeField] protected Color colorChase;
    [SerializeField] protected Color colorMotionless;
    [SerializeField] protected Vector2 eyeColorIntensity;

    [Header("Damage Display")]
    [SerializeField] protected TMP_Text hitValueDisplay;
    [SerializeField] private float durationDelay;
    [SerializeField] private float durationDotween;
    protected TweenerCore<Vector3, Vector3, VectorOptions> dotween;
    public GameObject deathVFX;
    public GameObject stunVFX;
    public GameObject hitVFX;

    [Header("Neutral Ult Display")]
    [SerializeField] protected GameObject UltIndicator;
    private GameObject stunZone = null;
    public Action<EnnemyBase> OnDeath;
    
    [Header("Life display")]
    [SerializeField] private GameObject lifeBar;
    [SerializeField] private Image frontLife;
    [SerializeField] private Image dmgLife;
    [SerializeField] private float bounceDuration;
    private float _tempHP;
    private float maxHP;
    [Tooltip("value when HP = 0")]
    [Range(0, 1)][SerializeField] private float minFillAmount = 0.1f;
    [Tooltip("value when HP = Maximum")]
    [Range(0, 1)][SerializeField] private float maxFillAmount = 0.9f;

    protected virtual void Start()
    {
        move = "patrol";
        EnnemyManager.Instance.enemies.Add(this);

        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        colorNormal *= eyeColorIntensity.x; colorChase *= eyeColorIntensity.y;
        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
        if (UltIndicator) UltIndicator.SetActive(false);
        EyesSetColorTo(colorNormal);

        HP = data.health;
        _tempHP = HP;
        maxHP = HP;
        speed = new Vector2(data.speed, data.chasespeed);
        SpeedRotate = new Vector2(data.speedRotate, data.chasespeedRotate);
        acceleration = new Vector2(data.acceleration, data.chaseacceleration);

        if (Player == null)
        {
            GameObject[] possiblePlayer = GameObject.FindGameObjectsWithTag("Player");
            Player = possiblePlayer[0].transform;
        }

        CurrentTarget = Player;
    }

    protected virtual void FixedUpdate()
    {
        if (move == "sleep")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                animator.SetBool("Sleep", false);
            }
        }
        if (move == "stun")
        {
            timerGeneral -= Time.deltaTime;
            if (timerGeneral <= 0)
            {
                EndStun();
            }
        }

        if (stunZone != null)
        {
            if (!stunZone.activeInHierarchy)
            {
                EndStun();
                stunZone = null;
            }
        }
    }

    public void StartSleep(float timer)
    {
        timerGeneral = timer;
        move = "sleep";

        if (animator != null)
        {
            EyesSetColorTo(Color.black);
            animator.SetBool("Sleep", true);
        }
    }

    protected void EndSleep()
    {
        move = "chase";
        EyesSetColorTo(colorNormal);
    }

    protected void EyesSetColorTo(Color color)
    {
        if (Eyes.Count > 0)
        {
            foreach (MeshRenderer eye in Eyes) eye.material.color = color;
        }
    }

    public virtual void TakeDamage(int damage, float stun)
    {
        if (dotween != null)
        {
            dotween.Kill();
            if (hitValueDisplay) hitValueDisplay.transform.localScale = Vector3.zero;
        }

        dotween = null;
        if (hitValueDisplay)
        {
            if (invincible)
            {
                if (showDamageDisplayInvincible) hitValueDisplay.text = damage.ToString();
                else hitValueDisplay.text = "Nope";
            }
            else hitValueDisplay.text = damage.ToString();

            ShowHitDisplay();
        }

        if (!invincible)
        {
            if (!lifeBar.activeSelf)
            {
                lifeBar.SetActive(true);
                lifeBar.transform.DOScale(1.2f, bounceDuration)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() =>
                    {
                        lifeBar.transform.DOScale(1f, bounceDuration)
                            .SetEase(Ease.InBounce);
                    });
            }
            float targetHP = (float)Math.Round((decimal)(HP - damage), 2);
            HP -= damage;
            hitVFX.transform.SetParent(transform.parent);
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
            hitVFX.transform.LookAt(lookTarget);
            hitVFX.transform.Rotate(0, 90, 0);

            hitVFX.SetActive(true);
            StartCoroutine(VisualDamage(targetHP));
        }

        if (HP <= 0)
        {
            Death();
        }
        else
        {
            if (move == "sleep")
            {
                if (animator == null) EndSleep();
                else animator.SetBool("Sleep", false);
            }
            else
            {
                if (move != "attack") move = "chase";
            }

            if (stun > 0 && !invincible) StunEnnemy(stun * stunMultiplier, false);
        }
    }

    public void SetUltIndicator(bool value)
    {
        if (UltIndicator) UltIndicator.SetActive(value);
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("StunZone"))
        {
            StunEnnemy(0f, true);
            if (stunZone != null) stunZone = other.gameObject;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StunZone"))
        {
            EndStun();
        }
    }

    protected void ShowHitDisplay()
    {
        if (hitValueDisplay == null || hitValueDisplay.gameObject == null) return;

        dotween = hitValueDisplay.transform.DOScale(1f, durationDotween)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                if (hitValueDisplay != null)
                {
                    hitValueDisplay.transform.DOScale(0f, durationDotween)
                        .SetEase(Ease.OutBounce)
                        .SetDelay(durationDelay);
                }
            });
    }

    protected virtual void Death()
    {
        lifeBar.SetActive(false);
        dotween?.Kill(); 
        transform.DOKill();
        deathVFX.SetActive(true);
        
        if (EnnemyManager.Instance != null)
        {
            EnnemyManager.Instance.enemies.Remove(this);
            EnnemyManager.Instance.Check();
        }
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public virtual void AttackStart(int attackID)
    {
        EyesSetColorTo(colorChase);
        move = "attack";
        animator.SetInteger("Attack", attackID);
    }

    public virtual void AttackAnimEnd()
    {
        animator.SetInteger("Attack", 0);
    }

    public void ToogleMainAttack(int toogle)
    {
        if (toogle == 1) MainHitBox.ToggleHitBox(true);
        else MainHitBox.ToggleHitBox(false);
    }

    public virtual void StunEnnemy(float stunTime, bool infiniteStun)
    {
        MusicManager.Instance.PlayStun();
        stunVFX.SetActive(true);
        EyesSetColorTo(colorMotionless);
        ToogleMainAttack(-1);
        move = "stun";
        timerGeneral = infiniteStun ? Mathf.Infinity : stunTime;

    }

    protected virtual void EndStun()
    {
        MusicManager.Instance.StopStun();
        stunVFX.SetActive(true);
        EyesSetColorTo(colorNormal);
        animator.SetBool("Stun", false);
        timerGeneral = 0;
        move = "0";
    }

    public string move
    {
        get => _move;
        set
        {
            if (_move == value) return;

            _move = value;

            if (EnnemyManager.Instance != null)
            {
                EnnemyManager.Instance.Check();
            }
        }
    }

    private IEnumerator VisualDamage(float newLife)
    {
        while (_tempHP > newLife)
        {
            float nextHP = Mathf.MoveTowards(_tempHP, newLife, 50 * Time.deltaTime);
            _tempHP = (float)Math.Round(nextHP, 2);

            UpdateVisuals();
            yield return null;
        }
    }
    
    private void UpdateVisuals()
    {
        frontLife.fillAmount = NormalizeValue(HP);
        dmgLife.fillAmount = NormalizeValue(_tempHP);
    }
    
    private float NormalizeValue(float value)
    {
        float lifeRatio = Mathf.Clamp01(value / (float)maxHP);
        return Mathf.Lerp(minFillAmount, maxFillAmount, lifeRatio);
    }
}
