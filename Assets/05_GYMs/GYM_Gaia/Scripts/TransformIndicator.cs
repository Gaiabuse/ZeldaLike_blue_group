using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TransformIndicator : MonoBehaviour
{
    [SerializeField] private Image formIcon;
    [SerializeField] private Image spellIndicator;
    [FormerlySerializedAs("iconsSprites")]
    [Tooltip("order : 0= neutre 1 = cauchemar 2 = onirique")]
    [SerializeField] private Sprite[] formIconSpr;
    
    [SerializeField] private GameObject neutralSpell;
    [SerializeField] private GameObject createIcon;
    [SerializeField] private GameObject eraseIcon;
    [SerializeField] private GameObject[] chargesIcon;
    
    [SerializeField] private GameObject nightmareSpell;
    [SerializeField] private GameObject grabIcon;
    [SerializeField] private GameObject eatIcon;
    [SerializeField] private GameObject spitIcon;
    
    [SerializeField] private GameObject dreamSpell;
    [SerializeField] private GameObject baitIcon;
    [SerializeField] private GameObject explodeIcon;
    
    [Tooltip("order : 0= l1 1= r1")]
    [SerializeField] private FormSwitcher formSwitcher;

    private bool hasToBlink;
    private Coroutine blinkCoroutine;
    private Coroutine fadeCoroutine;
    
    public static TransformIndicator Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        SwitchIndicators(Form.neutral);
        FormSwitcher.SwitchForm += SwitchIndicators;
    }

    private void OnDisable()
    {
        FormSwitcher.SwitchForm -= SwitchIndicators;
    }

    private void SwitchIndicators(Form currentForm)
    {
        switch (currentForm)
        {
            case Form.neutral:
                formIcon.sprite = formIconSpr[0];
                neutralSpell.SetActive(true);
                nightmareSpell.SetActive(false);
                dreamSpell.SetActive(false);
                break;
            case Form.nightmare:
                formIcon.sprite = formIconSpr[1];
                neutralSpell.SetActive(false);
                nightmareSpell.SetActive(true);
                dreamSpell.SetActive(false);
                break;
            case Form.dream:
                formIcon.sprite = formIconSpr[2];
                neutralSpell.SetActive(false);
                nightmareSpell.SetActive(false);
                dreamSpell.SetActive(true);
                break;
        }
    }

    public void DisplayBaitIcon()
    {
        baitIcon.SetActive(true);
        explodeIcon.SetActive(false);
    }

    public void DisplayExplodeIcon()
    {
        explodeIcon.SetActive(true);
        baitIcon.SetActive(false);
    }

    public void DisplayNightmareIcon(int icon)
    {
        switch (icon)
        {
            case 0:
                grabIcon.SetActive(true);
                eatIcon.SetActive(false);
                spitIcon.SetActive(false);
                break;
            case 1:
                grabIcon.SetActive(false);
                eatIcon.SetActive(true);
                spitIcon.SetActive(false);
                break;
            case 2:
                grabIcon.SetActive(false);
                eatIcon.SetActive(false);
                spitIcon.SetActive(true);
                break;
        }
    }

    public void DisplayNeutralIcon(int icon)
    {
        if (icon == 0)
        {
            createIcon.SetActive(true);
            eraseIcon.SetActive(false);
        }
        else
        {
            createIcon.SetActive(false);
            eraseIcon.SetActive(true);
        }
    }

    public void DisplayNeutralChargeIcon(int icon)
    {
        if (hasToBlink) return;
        for (int i = 0; i < chargesIcon.Length; i++)
        {
            chargesIcon[i].SetActive(i == icon-1); 
        }
    }
    
    public void StartBlink(int i)
    {
        hasToBlink = true;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkNeutralChargeIcon(i));
    }
    public void StopBlink(int iconIndex)
    {
        int index = iconIndex+(ErasedManager.Instance.currentPointsForCreate-3);
        hasToBlink = false;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        if (index+1 < 0) return;
        var cg = chargesIcon[index+1].GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
        }
    }

    public IEnumerator BlinkNeutralChargeIcon(int iconIndex)
    {
        int index = iconIndex+(ErasedManager.Instance.currentPointsForCreate-3);
        CanvasGroup canvasGroupOut = chargesIcon[index+1].GetComponent<CanvasGroup>();
        if (index >= 0)
        {
            chargesIcon[index].SetActive(true);
            CanvasGroup cg = chargesIcon[index].GetComponent<CanvasGroup>();
            if (cg == null) cg = chargesIcon[index].AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            chargesIcon[index].SetActive(true);
        }
        
        if (canvasGroupOut == null) canvasGroupOut = chargesIcon[index+1].AddComponent<CanvasGroup>();

        hasToBlink = true;
        float fadeDuration = 0.5f;
        
        while (hasToBlink)
        {
            yield return fadeCoroutine = StartCoroutine(FadeAlpha(canvasGroupOut, 0, 1, fadeDuration));
            yield return fadeCoroutine = StartCoroutine(FadeAlpha(canvasGroupOut, 1, 0, fadeDuration));
        }
    }
    
    private IEnumerator FadeAlpha(CanvasGroup cg, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            
            yield return null;
        }
        cg.alpha = end;
    }
}
