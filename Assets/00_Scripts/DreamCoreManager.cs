using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DreamCoreManager : MonoBehaviour
{
    [SerializeField] private PlayerController Player;
    [SerializeField] private int hp = 1000;
    
    [Header("Damage Display")]
    [SerializeField] protected GameObject hitVFX;
    
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

    private void Start()
    {
        maxHP = hp;
        _tempHP = maxHP;
    }

    public void TakeDamages(int damage)
    {
        if (hp > 0)
        {
            if (!lifeBar.activeSelf)
            {
                lifeBar.SetActive(true);
                lifeBar.transform.DOScale(1f, bounceDuration).SetEase(Ease.OutBounce);
            }
            float targetHP = (float)Math.Round((decimal)(hp - damage), 2);
            hp -= damage;
            hitVFX.transform.SetParent(transform.parent);
            hitVFX.transform.position = transform.position;
            Vector3 lookTarget = new Vector3(Player.transform.position.x, hitVFX.transform.position.y, Player.transform.position.z);
            hitVFX.transform.LookAt(lookTarget);
            hitVFX.transform.Rotate(0, 90, 0);

            hitVFX.SetActive(true);
            StartCoroutine(VisualDamage(targetHP));

            if (hp <= 0)
            {
                lifeBar.SetActive(false);
                hitVFX.SetActive(false);
                //Death();
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
        frontLife.fillAmount = NormalizeValue(hp);
        dmgLife.fillAmount = NormalizeValue(_tempHP);
    }
    
    private float NormalizeValue(float value)
    {
        float lifeRatio = Mathf.Clamp01(value / (float)maxHP);
        return Mathf.Lerp(minFillAmount, maxFillAmount, lifeRatio);
    }
}
