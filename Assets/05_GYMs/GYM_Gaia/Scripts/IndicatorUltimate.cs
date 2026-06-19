using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class IndicatorUltimate : MonoBehaviour
{
    [SerializeField] private float dotweenTime = 0.1f;
    [SerializeField] private float bounceMinScale = 0.9f;
    [SerializeField] private float bounceMaxScale = 1.1f;
    [SerializeField] private float bounceDuration = 0.5f;
    [SerializeField] private GameObject indicator;

    private Tween tween;
    private bool isShow;
    private float indicatorSize;

    private void OnEnable()
    {
        AttackManager.CanUltimate += ShowIndicator;
        AttackManager.EndForUltimate += HideIndicator;
    }

    private void OnDisable()
    {
        AttackManager.CanUltimate -= ShowIndicator;
        AttackManager.EndForUltimate -= HideIndicator;
    }

    private void Start()
    {
        indicatorSize = indicator.transform.localScale.x;
    }

    private void ShowIndicator()
    {
        if (indicator == null || isShow) return;
        isShow = true;

        indicator.SetActive(true);
        indicator.transform.localScale = Vector3.zero;

        tween?.Kill();

        tween = indicator.transform
            .DOScale(indicatorSize * bounceMaxScale, dotweenTime)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(StartBounceLoop);
    }

    private void StartBounceLoop()
    {
        tween?.Kill();

        tween = indicator.transform
            .DOScale(indicatorSize * bounceMinScale, bounceDuration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HideIndicator()
    {
        if (indicator == null) return;
        isShow = false;

        tween?.Kill();

        tween = indicator.transform
            .DOScale(Vector3.zero, dotweenTime)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => indicator.SetActive(false));
    }
}