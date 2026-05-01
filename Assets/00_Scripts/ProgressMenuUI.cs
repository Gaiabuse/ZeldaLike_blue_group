using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ProgressMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject progressMenu;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject pauseSfxTrigger;
    [SerializeField] private Image progressSlider;
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
    
    private TweenerCore<Vector2, Vector2, VectorOptions> pauseDotween;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
        SetMilestonesPosition(milestones);
        SetMilestonesPosition(milestonesPopUp);
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
        progressMenu.SetActive(true);
        
        RectTransform rect = progressMenu.GetComponent<RectTransform>();
    
        if (pauseDotween != null) pauseDotween.Kill();
        
        pauseDotween = rect.DOAnchorPos(Vector2.zero, 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Time.timeScale = 0;
                UpdatePhoneInfos(progressSlider, progressAnimGO);
                AnimateSpriteSheet(progressAnim);
            });
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
                player.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
            });
    }

    private void UpdatePhoneInfos(Image slider, GameObject anim)
    {
        float targetFill = QuotaManager.Instance.cleanPoints;
        targetFill /= 100f;
        slider.DOFillAmount(targetFill, 1.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic);
        
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
}
