using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject settingsFirstSelectedButton;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pauseSfxTrigger;
    
    private enum MenuState { Playing, Pause, Settings }
    private MenuState currentState = MenuState.Playing;
    
    private TweenerCore<float, float, FloatOptions> pauseDotween;
    private TweenerCore<float, float, FloatOptions> settingsDotween;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    public void ClosePauseMenu()
    {
        pauseSfxTrigger.SetActive(false);
        MusicManager.Instance.PlayCancel();
        Time.timeScale = 1;
        if (currentState == MenuState.Settings)
        {
            if (settingsDotween != null)
            {
                settingsDotween.Kill();
            }
            settingsDotween = settingsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    settingsScreen.SetActive(false);
                });
        }
        
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = pauseMenu.GetComponent<CanvasGroup>().DOFade(0f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                pauseMenu.SetActive(false);
                player.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
            });
        
        currentState = MenuState.Playing;
    }

    public void OpenPauseMenu()
    {
        pauseSfxTrigger.SetActive(true);
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = pauseMenu.GetComponent<CanvasGroup>().DOFade(1f, 0.5f)
            .SetUpdate(true)
            .OnComplete(() =>
        {
            Time.timeScale = 0;
        });
    }

    public void OpenSettings()
    {
        MusicManager.Instance.PlayClick();
        
        currentState = MenuState.Settings;
        settingsScreen.SetActive(true);
        
        if (settingsDotween != null)
        {
            settingsDotween.Kill();
        }
        settingsDotween = settingsScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.25f)
            .SetUpdate(true)
            .OnComplete(() =>
        {
            pauseMenu.SetActive(false);
            EventSystem.current.SetSelectedGameObject(settingsFirstSelectedButton);
        });
    }

    public void CloseSettings()
    {
        MusicManager.Instance.PlayCancel();
        
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        
        if (settingsDotween != null)
        {
            settingsDotween.Kill();
        }
        settingsDotween = settingsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f)
            .SetUpdate(true)
            .OnComplete(() =>
        {
            settingsScreen.SetActive(false);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        });
    }
    
    public void Restart()
    {
        player.TriggerRespawn();
        ClosePauseMenu();
        MusicManager.Instance.PlayClick();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        StartCoroutine(LoadMainMenuSequence());
        MusicManager.Instance.PlayClick();
    }

    public void Quit()
    {
        Application.Quit();
        MusicManager.Instance.PlayClick();
    }

    public void Return()
    {
        MusicManager.Instance.PlayCancel();
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
    
    private IEnumerator<WaitForSeconds> LoadMainMenuSequence()
    {
        StartCoroutine(RumbleCoroutine(0.5f, 0.5f, 0.5f));
        
        loadingScreen.SetActive(true);
        loadingScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        
        // WaitForSecond have to be longer than the rumbling duration to avoid endless rumbling
        yield return new WaitForSeconds(1.5f);
        
        AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu");
        while (operation != null && !operation.isDone)
        {
            yield return null; 
        }
    }
    

    private IEnumerator<WaitForSeconds> RumbleCoroutine(float duration, float low, float high) {
        Gamepad.current.SetMotorSpeeds(low, high);
        yield return new WaitForSeconds(duration);
        Gamepad.current.SetMotorSpeeds(0f, 0f);
    }
}
