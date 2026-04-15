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
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pauseSfxTrigger;
    
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
        pauseSfxTrigger.SetActive(false);
        Time.timeScale = 1;
        if (pauseDotween != null)
        {
            pauseDotween.Kill();
        }
        pauseDotween = pauseMenu.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            pauseMenu.SetActive(false); 
        });
        currentState = MenuState.Playing;
        player.GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerControl");
    }

    public void OpenPauseMenu()
    {
        pauseSfxTrigger.SetActive(true);
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    
        // 2. Set the default button
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
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
        StartCoroutine(LoadMainMenuSequence());
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
