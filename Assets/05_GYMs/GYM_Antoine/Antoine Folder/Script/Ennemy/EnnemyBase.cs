using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnnemyBase : MonoBehaviour
{
    protected Animator animator;

    [Header("Data")]
    [SerializeField] private EnemyData data;

    protected int HP = 5;
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

    [SerializeField] protected string move = "0";
    protected float timerGeneral = 0;

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

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();

        colorNormal *= eyeColorIntensity.x; colorChase *= eyeColorIntensity.y;
        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
        EyesSetColorTo(colorNormal);

        HP = data.health;
        speed = new Vector2(data.speed, data.chasespeed);
        SpeedRotate = new Vector2(data.speedRotate, data.chasespeedRotate);
        acceleration = new Vector2(data.acceleration, data.chaseacceleration);
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

    public virtual void TakeDamage(int damage)
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

        if (!invincible) HP -= damage;

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
        }
    }

    protected void ShowHitDisplay()
    {
        if (hitValueDisplay)
        {
            dotween = hitValueDisplay.transform.DOScale(1f, durationDotween).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                hitValueDisplay.transform.DOScale(0f, durationDotween).SetEase(Ease.OutBounce).SetDelay(durationDelay);
            });
        }
    }

    protected virtual void Death()
    {
        Destroy(gameObject);
    }

    protected virtual void AttackStart(int attackID)
    {
        EyesSetColorTo(colorChase);

        move = "attack";
        animator.SetInteger("Attack", attackID);
    }

    protected virtual void AttackAnimEnd()
    {
        animator.SetInteger("Attack", 0);
    }

    protected void ToogleMainAttack(int toogle)
    {
        if (toogle == 1) MainHitBox.ToggleHitBox(true);
        else MainHitBox.ToggleHitBox(false);
    }
}
