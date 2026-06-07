using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ProgressMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject progressMenu;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject pauseSfxTrigger;
    [SerializeField] private Image progressSlider;
    [SerializeField] private TMP_Text progressPercentage;
    [SerializeField] private GameObject progressAnimGO;
    [SerializeField] private Vector2 animMinMaxPosX;
    [SerializeField] private Image progressAnim;
    [SerializeField] private Sprite[] animSprites;
    [SerializeField] private Vector2 milestonesMinMaxPosX;
    [SerializeField] private RectTransform[] milestones;
    [SerializeField] private GameObject progressPopUp;
    [SerializeField] private Image progressAnimPopUp;
    [SerializeField] private Image progressSliderPopUp;
    [SerializeField] private GameObject progressAnimGOPopUp;
    [SerializeField] private RectTransform[] milestonesPopUp;
    [SerializeField] private GameObject progressToggle;
    [SerializeField] private GameObject messageToggle;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private TMP_Text playTimeText;

    private float playTime;
    
    private TweenerCore<Vector2, Vector2, VectorOptions> pauseDotween;
    private bool isProgessShown = true;
    
    private void Start()
    {
        /*Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;*/
        SetMilestonesPosition(milestones);
        SetMilestonesPosition(milestonesPopUp);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            float time = GameManager.Instance.playTime;

            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);

            playTimeText.text = $"{hours:00}h {minutes:00}m {seconds:00}s";
        }
    }

    private void SetMilestonesPosition(RectTransform[] milestones)
    {
        for (int i = 0; i < milestones.Length; i++)
        {
            float t = QuotaManager.Instance.quotas[i] / 100f;
            float targetX = Mathf.Lerp(milestonesMinMaxPosX.x, milestonesMinMaxPosX.y, t);
            
            Vector2 newPos = milestones[i].anchoredPosition;
            newPos.x = targetX;
            milestones[i].anchoredPosition = newPos;
        }
    }

    public void OpenProgressMenu()
    {
        pauseSfxTrigger.SetActive(true);
    
        RectTransform rect = progressMenu.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -800f); 
    
        progressMenu.SetActive(true);
    
        if (pauseDotween != null) pauseDotween.Kill();
    
        pauseDotween = rect.DOAnchorPos(Vector2.zero, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Time.timeScale = 0;
                UpdatePhoneInfos(progressSlider, progressAnimGO);
                AnimateSpriteSheet(progressAnim);
                player.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap("ProgressControl");
            });

        StartCoroutine(ResetScrollbarRoutine());
    }

    public void CloseProgressMenu()
    {
        pauseSfxTrigger.SetActive(false);
        Time.timeScale = 1;
    
        RectTransform rect = progressMenu.GetComponent<RectTransform>();

        if (pauseDotween != null) pauseDotween.Kill();
        
        pauseDotween = rect.DOAnchorPos(new Vector2(0, -800f), 0.5f)
            .SetEase(Ease.InBack) 
            .OnComplete(() =>
            {
                progressMenu.SetActive(false);
                player.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
            });
    }

    private void UpdatePhoneInfos(Image slider, GameObject anim)
    {
        float targetFill = QuotaManager.Instance.cleanPoints;
        targetFill /= 100f;
        slider.DOFillAmount(targetFill, 1.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic);
        progressPercentage.text = $"{targetFill*100:0.0}%";
        
        float targetX = Mathf.Lerp(animMinMaxPosX.x, animMinMaxPosX.y, targetFill);
        anim.GetComponent<RectTransform>().DOAnchorPosX(targetX, 1.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic);
    }
    
    private void AnimateSpriteSheet(Image image)
    {
        DOTween.To(() => 0f, x => {
                int index = Mathf.FloorToInt(x % animSprites.Length);
                image.sprite = animSprites[index];
            }, animSprites.Length, 0.5f)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .SetEase(Ease.Linear);
    }

    public void ShowProgressPopUp()
    {
        CanvasGroup cg = progressPopUp.GetComponent<CanvasGroup>();
        progressPopUp.SetActive(true);
        cg.alpha = 0f; 
        Sequence sequence = DOTween.Sequence();
        sequence.Append(cg.DOFade(1f, 0.5f).SetUpdate(true));
        sequence.AppendInterval(2f);
        sequence.Append(cg.DOFade(0f, 0.5f).SetUpdate(true));
        sequence.OnComplete(() => progressPopUp.SetActive(false));
        AnimateSpriteSheet(progressAnimPopUp);
        UpdatePhoneInfos(progressSliderPopUp, progressAnimGOPopUp);
    }

    public void SwitchToggle()
    {
        MusicManager.Instance.PlayClick();
        isProgessShown = !isProgessShown;
        if (isProgessShown)
        {
            progressToggle.SetActive(true);
            messageToggle.SetActive(false);
        }
        else
        {
            progressToggle.SetActive(false);
            messageToggle.SetActive(true);
        
            // FIX 3: Force scrollbar reset safely here too
            StartCoroutine(ResetScrollbarRoutine());
        }
    }
    
    private IEnumerator ResetScrollbarRoutine()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if (scrollbar != null)
        {
            scrollbar.value = 0f;
        }
    }

    public void Scroll(float value)
    {
        scrollbar.value += value * scrollSpeed * Time.unscaledDeltaTime;
        scrollbar.value = Mathf.Clamp01(scrollbar.value);
    }
}
