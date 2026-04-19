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
    
    private TweenerCore<float, float, FloatOptions> pauseDotween;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    public void CloseProgressMenu()
    {
        pauseSfxTrigger.SetActive(false);
        Time.timeScale = 1;
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = progressMenu.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            progressMenu.SetActive(false);
            player.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
        });
    }

    public void OpenProgressMenu()
    {
        pauseSfxTrigger.SetActive(true);
        progressMenu.SetActive(true);
        
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = progressMenu.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).OnComplete(() =>
        {
            Time.timeScale = 0;
            UpdatePhoneInfos();
            AnimateSpriteSheet();
        });
    }

    private void UpdatePhoneInfos()
    {
        float targetFill = QuotaManager.Instance.cleanPoints;
        targetFill /= 100f;
        progressSlider.DOFillAmount(targetFill, 1.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic);
        
        float targetX = Mathf.Lerp(-125f, 105f, targetFill);
        progressAnimGO.GetComponent<RectTransform>().DOAnchorPosX(targetX, 1.5f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic);
    }
    
    private void AnimateSpriteSheet()
    {
        DOTween.To(() => 0f, x => {
                int index = Mathf.FloorToInt(x % animSprites.Length);
                progressAnim.sprite = animSprites[index];
            }, animSprites.Length, 0.5f)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true)
            .SetEase(Ease.Linear);
    }
}
