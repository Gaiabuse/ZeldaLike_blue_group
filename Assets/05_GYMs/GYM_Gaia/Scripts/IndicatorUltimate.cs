using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;
using UnityEngine;

public class IndicatorUltimate : MonoBehaviour
{
    [SerializeField] private float dotweenTime = 0.1f;
    [SerializeField] private GameObject indicator;
    private TweenerCore<Vector3, Vector3, VectorOptions> tween;
    private bool isShow;

    private void OnEnable()
    {
        AttackManager.CanUltimate += ShowIndicator;
        AttackManager.EndForUltimate += HideIndicator;
    }


    private void Start()
    {
        indicator.transform.localScale = Vector3.zero;
    }

    private void ShowIndicator()
    {
        if (!indicator && isShow) return;
        isShow = true;
        indicator.SetActive(true);
        if (tween != null) tween.Kill();
        tween = indicator.transform.DOScale(Vector3.one, dotweenTime).SetEase(Ease.OutBounce);
    }

    private void HideIndicator()
    {
        if (!indicator && !isShow) return;
        if (tween != null) tween.Kill();
        tween = indicator.transform.DOScale(Vector3.zero, dotweenTime).SetEase(Ease.InBounce);
    }
}
