using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Textbox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] float delayBeforeDisappear = 1f;
    [SerializeField] float tweenDuration = 0.2f;
    [SerializeField] GameObject phone;
    string textShow;
    
    TweenerCore<Vector3, Vector3, VectorOptions> showTextTween;

    TweenerCore<Vector3, Vector3, VectorOptions> hideTextTween;
    private void Start()
    {
        textBox.text = null;
        transform.localScale = Vector3.zero;
    }
    

    public void AppearText(string text)
    {
        if (hideTextTween != null)
        {
            hideTextTween.Kill();
            transform.localScale = Vector3.zero;
        }
        if (showTextTween != null)
        {
            showTextTween.Kill();
        }

        StartCoroutine(NotificationAnim());
        showTextTween = transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBounce);
        textBox.text = null;
        textShow = text;
        textBox.text = textShow;
        hideTextTween = transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBounce).SetDelay(delayBeforeDisappear + tweenDuration).OnComplete(
            () =>
            {
                phone.SetActive(false);
            });
    }

    private IEnumerator NotificationAnim()
    {
        phone.SetActive(true);
        MusicManager.Instance.RingPhone();
        for (int i = 0; i < 3; i++)
        {
            Gamepad.current.SetMotorSpeeds(0.25f, 0.25f);
            phone.transform.DOShakeRotation(0.22f, new Vector3(0f, 0f, 20f)).OnComplete(() =>
            {
                Gamepad.current.SetMotorSpeeds(0f, 0f); 
            });
            yield return new WaitForSeconds(0.33f);
        }
    }
}
