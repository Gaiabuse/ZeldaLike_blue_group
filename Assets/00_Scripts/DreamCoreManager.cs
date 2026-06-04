using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DreamCoreManager : MonoBehaviour
{
    [SerializeField] private PlayerController Player;
    [SerializeField] private int hp = 1000;

    [Header("ArenaTriggers")] 
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private GameObject arenaObjects;
    [SerializeField] private float timeForStarFight = 0.5f;
    
    [Header("Damage Display")] [SerializeField]
    protected GameObject hitVFX;

    [Header("Life display")] [SerializeField]
    private GameObject lifeBar;
    [SerializeField] private Image frontLife;
    [SerializeField] private Image dmgLife;
    [SerializeField] private float bounceDuration;
    private float _tempHP;
    private float maxHP;
    [Tooltip("value when HP = 0")] [Range(0, 1)] [SerializeField]
    private float minFillAmount = 0.1f;
    [Tooltip("value when HP = Maximum")] [Range(0, 1)] [SerializeField]
    private float maxFillAmount = 0.9f;

    [Header("Goo Size Display")] [Tooltip("size of goo when HP = 0")] [Range(0, 2)] [SerializeField]
    private float minGooSize = 0.1f;
    [Tooltip("size of goo HP = Maximum")] [Range(0, 2)] [SerializeField]
    private float maxGooSize = 0.9f;
    [SerializeField] private float gooLerpDuration = 0.2f;

    private void Start()
    {
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
}