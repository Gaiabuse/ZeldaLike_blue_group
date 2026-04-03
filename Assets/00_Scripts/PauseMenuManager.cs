using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private PlayerController player;
    
    private enum MenuState { Playing, Pause, Settings }
    private MenuState currentState = MenuState.Playing;
    
    private TweenerCore<float, float, FloatOptions> pauseDotween;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1;
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = pauseMenu.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            pauseMenu.SetActive(false); 
        });
        currentState = MenuState.Playing;
    }

    public void OpenPauseMenu()
    {
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = pauseMenu.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).OnComplete(() =>
        {
            Time.timeScale = 0;
        });
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings /not implemented");
        
    }
    
    public void CloseSettings()
    {
        Debug.Log("CloseSettings /not implemented");
        
    }
    
    public void Restart()
    {
        player.TriggerRespawn();
        ClosePauseMenu();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Return()
    {
        switch (currentState)
        {
            case MenuState.Pause:
                ClosePauseMenu();
                break;
            
            case MenuState.Settings:
                CloseSettings();
                break;
        }
    }
}
