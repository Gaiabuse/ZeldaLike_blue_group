using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TransformIndicator : MonoBehaviour
{
    [SerializeField] private Image formIcon;
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
    public Image createIconImg;
    public Image createPointsIconeateIconImg;

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
    // Don't interrupt if a blink is actively overriding the UI layout
    if (hasToBlink) return; 
    
    for (int i = 0; i < chargesIcon.Length; i++)
    {
        chargesIcon[i].SetActive(i == icon - 1); 
        
        // Ensure alpha is fully reset to visible when updated normally
        CanvasGroup cg = chargesIcon[i].GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
    }
}

public void StartBlink(int cost)
{
    int currentPoints = ErasedManager.Instance.currentPointsForCreate;

    // Rule: If less points than needed, don't blink
    if (currentPoints < cost)
    {
        StopBlink();
        return;
    }

    hasToBlink = true;
    if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    blinkCoroutine = StartCoroutine(BlinkNeutralChargeIconRoutine(currentPoints, cost));
}

public void StopBlink(int cost = 0) // Kept the optional parameter so ErasedObject script doesn't break
{
    hasToBlink = false;
    if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

    // Revert UI back to reflecting exactly what the player currently holds
    int currentPoints = ErasedManager.Instance.currentPointsForCreate;
    for (int i = 0; i < chargesIcon.Length; i++)
    {
        CanvasGroup cg = chargesIcon[i].GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
        
        chargesIcon[i].SetActive(i == currentPoints - 1);
    }
}

private IEnumerator BlinkNeutralChargeIconRoutine(int currentPoints, int cost)
{
    int sourceIndex = currentPoints - 1;       // The stacked group that blinks
    int targetIndex = currentPoints - cost - 1; // The remaining points layer showing underneath

    // Deactivate everything first to establish a clean state
    for (int i = 0; i < chargesIcon.Length; i++)
    {
        chargesIcon[i].SetActive(false);
    }

    // 1. Activate the background 'target' point layer (e.g., if 3 points and cost 1, show 2 points statically)
    if (targetIndex >= 0 && targetIndex < chargesIcon.Length)
    {
        chargesIcon[targetIndex].SetActive(true);
        CanvasGroup targetCg = chargesIcon[targetIndex].GetComponent<CanvasGroup>();
        if (targetCg == null) targetCg = chargesIcon[targetIndex].AddComponent<CanvasGroup>();
        targetCg.alpha = 1f;
    }

    // 2. Activate the foreground 'source' layer and start blinking it
    if (sourceIndex >= 0 && sourceIndex < chargesIcon.Length)
    {
        chargesIcon[sourceIndex].SetActive(true);
        CanvasGroup sourceCg = chargesIcon[sourceIndex].GetComponent<CanvasGroup>();
        if (sourceCg == null) sourceCg = chargesIcon[sourceIndex].AddComponent<CanvasGroup>();
        sourceCg.alpha = 1f;

        float fadeDuration = 0.35f; // Slightly accelerated for a snappier UI feel
        
        while (hasToBlink)
        {
            yield return fadeCoroutine = StartCoroutine(FadeAlpha(sourceCg, 1f, 0f, fadeDuration));
            yield return fadeCoroutine = StartCoroutine(FadeAlpha(sourceCg, 0f, 1f, fadeDuration));
        }
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
