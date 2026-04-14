using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] public int maxHP = 15;
    [SerializeField] private PlayerController playerController;
    public Action OnTakeDamage;
    [SerializeField] private float speedRecharge;

    [Header("Settings Visuals")]
    [Tooltip("value when HP = 0")]
    [Range(0, 1)] [SerializeField] private float minFillAmount = 0.1f;
    [Tooltip("value when HP = Maximum")]
    [Range(0, 1)] [SerializeField] private float maxFillAmount = 0.9f;
    [SerializeField] Image healthBar;

    private Coroutine damageCoroutine;
    private Coroutine healCoroutine;
    public float HP;

    private void OnEnable()
    {
        ArenaManager.StartArena += StopHealing;
        ArenaManager.FinishArena += HealAtMax;
    }

    private void OnDisable()
    {
        ArenaManager.StartArena -= StopHealing;
        ArenaManager.FinishArena -= HealAtMax;
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void TakeDamage(int damage)
    {
        if (HP > 0)
        {
            if (damageCoroutine != null) StopCoroutine(damageCoroutine);
            if (healCoroutine != null) StopCoroutine(healCoroutine);

            float targetHP = (float)Math.Round(HP - damage, 2);
            damageCoroutine = StartCoroutine(VisualDamage(targetHP));
            Debug.Log("Outch");
        }
     
        OnTakeDamage?.Invoke();
    }

    private void StopHealing()
    {
        if (healCoroutine != null) StopCoroutine(healCoroutine);
    }

    private void HealAtMax()
    {
        Heal(maxHP - HP);
    }

    public void Heal(float heal)
    {
        if (HP >= maxHP) return;
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        float targetHP = (float)Math.Round(HP + heal, 2);
        healCoroutine = StartCoroutine(VisualHeal(targetHP));
    }

    private IEnumerator VisualDamage(float newLife)
    {
        while (HP > newLife)
        {
            float nextHP = Mathf.MoveTowards(HP, newLife, speedRecharge * Time.deltaTime);
            HP = (float)Math.Round(nextHP, 2);
            
            UpdateVisuals();

            if (HP <= 0)
            {
                StartCoroutine(playerController.RespawnCoroutine());
                HP = maxHP;
                UpdateVisuals();
                break;
            }
            yield return null;
        }
        damageCoroutine = null;
    }

    private IEnumerator VisualHeal(float newLife)
    {
        while (HP < newLife)
        {
            float nextHP = Mathf.MoveTowards(HP, newLife, speedRecharge * Time.deltaTime);
            HP = (float)Math.Round(nextHP, 2);

            UpdateVisuals();

            if (HP >= maxHP)
            {
                HP = maxHP;
                break;
            }
            yield return null;
        }
        healCoroutine = null;
    }
    
    private void UpdateVisuals()
    {
        healthBar.fillAmount = NormalizeValue(HP);
    }

    private float NormalizeValue(float value)
    {
        float lifeRatio = Mathf.Clamp01(value / (float)maxHP);
        return Mathf.Lerp(minFillAmount, maxFillAmount, lifeRatio);
    }
}