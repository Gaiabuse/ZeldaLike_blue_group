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

    private GameObject lastFocusedButton;

    private void Update()
    {
        if (currentState == MenuState.Playing) return;

        GameObject currentSelection = EventSystem.current.currentSelectedGameObject;

        if (currentSelection != null)
        {
            lastFocusedButton = currentSelection;
        }
        else
        {
            if (lastFocusedButton != null && lastFocusedButton.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastFocusedButton);
            }
        }
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
                player.transform.GetComponent<PlayerInput>().SwitchCurrentActionMap(InputManager.PLAYER_INPUT_MAP);
            });

        currentState = MenuState.Playing;
        lastFocusedButton = null;
    }

    public void OpenPauseMenu()
    {
        pauseSfxTrigger.SetActive(true);
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        lastFocusedButton = firstSelectedButton;

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
            lastFocusedButton = settingsFirstSelectedButton;
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
            lastFocusedButton = firstSelectedButton;
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

        yield return new WaitForSeconds(1.5f);

        AsyncOperation operation = SceneManager.LoadSceneAsync("MainMenu");
        while (operation != null && !operation.isDone)
        {
            yield return null;
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator<WaitForSeconds> RumbleCoroutine(float duration, float low, float high)
    {
        RumbleManager.Instance.TriggerVibration(low, high);
        yield return new WaitForSeconds(duration);
        RumbleManager.Instance.StopVibration();
    }
}