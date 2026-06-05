using UnityEngine;
using DG.Tweening;

public class TutoIndicatorBlink : MonoBehaviour
{
    private CanvasGroup _cg;
    private Tween _blinkTween;

    void Start()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg != null)
        {
            StartBlink();
        }
    }

    private void StartBlink()
    {
        _cg.alpha = 0f;
        _blinkTween = _cg.DOFade(1f, 1f)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopBlink()
    {
        if (_blinkTween != null && _blinkTween.IsActive())
        {
            _blinkTween.Kill();
        }
        
        if (_cg != null)
        {
            _cg.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}