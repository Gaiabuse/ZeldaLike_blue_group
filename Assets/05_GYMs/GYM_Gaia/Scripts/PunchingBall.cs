using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class PunchingBall : MonoBehaviour
{
    [SerializeField] private TMP_Text hitValueDisplay;
    [SerializeField] private float durationDelay;
    [SerializeField] private float durationDotween;
    private TweenerCore<Vector3, Vector3, VectorOptions> dotween;
    void Start()
    {
        hitValueDisplay.text = "";
        hitValueDisplay.transform.localScale = Vector3.zero;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            Attack attack = other.GetComponent<Attack>();
            
            TakeDamage(attack.damage);
        }
    }

    void TakeDamage(float damage)
    {
        if (dotween != null)
        {
            dotween.Kill();
            hitValueDisplay.transform.localScale = Vector3.zero;
        }

        dotween = null;
        hitValueDisplay.text = damage.ToString();
        ShowHitDisplay();
    }
    private void ShowHitDisplay()
    {
        dotween = hitValueDisplay.transform.DOScale(1f, durationDotween).SetEase(Ease.OutBounce).OnComplete(()=>
        {
            hitValueDisplay.transform.DOScale(0f, durationDotween).SetEase(Ease.OutBounce).SetDelay(durationDelay);
        });
    }
}
