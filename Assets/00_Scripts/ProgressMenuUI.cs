using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProgressMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject progressMenu;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject pauseSfxTrigger;
    
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
        });
    }
}
