using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Textbox : MonoBehaviour
{
    [SerializeField] Transform textBoxTransform;
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] float delayBeforeDisappear = 1f;
    [SerializeField] float tweenDuration = 0.2f;
    string textShow;
    
    TweenerCore<Vector3, Vector3, VectorOptions> showTextTween;

    TweenerCore<Vector3, Vector3, VectorOptions> hideTextTween;
    private void Start()
    {
        textBox.text = null;
        textBoxTransform.localScale = Vector3.zero;
    }
    

    public void AppearText(string text)
    {
        if (hideTextTween != null)
        {
            hideTextTween.Kill();
            textBoxTransform.localScale = Vector3.zero;
        }
        if (showTextTween != null)
        {
            showTextTween.Kill();
        }
        showTextTween = textBoxTransform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBounce);
        textBox.text = null;
        textShow = text;
        textBox.text = textShow;
        hideTextTween = textBoxTransform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBounce).SetDelay(delayBeforeDisappear + tweenDuration);
    }
    
}
