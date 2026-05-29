using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameScene;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject mainFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject creditsScreen;
    
    private enum MenuState { Title, Settings, Credits }
    private MenuState currentState = MenuState.Title;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    private void OnReturn()
    {
        switch (currentState)
        {
            case MenuState.Settings:
                CloseSettings();
                break;

            case MenuState.Credits:
                CloseCredits();
                break;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Play()
    {
        StartCoroutine(LaunchGameSequence());
    }
    
    public void ShowSettings()
    {
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            settingsScreen.SetActive(true);
            eventSystem.firstSelectedGameObject = settingsFirstSelected;
            eventSystem.SetSelectedGameObject(settingsFirstSelected);
            settingsScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Settings;
    }
    
    public void CloseSettings()
    {
        settingsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            settingsScreen.SetActive(false);
            titleScreen.SetActive(true);
            eventSystem.firstSelectedGameObject = mainFirstSelected;
            eventSystem.SetSelectedGameObject(mainFirstSelected);
            titleScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Title;
    }
    
    
    public void ShowCredits()
    {
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            creditsScreen.SetActive(true);
            creditsScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Credits;
    }
    
    public void CloseCredits()
    {
        creditsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            creditsScreen.SetActive(false);
            titleScreen.SetActive(true);
            titleScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Title;
    }
    
    private IEnumerator LaunchGameSequence()
    {
        StartCoroutine(RumbleCoroutine(0.5f, 0.5f, 0.5f));
        
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            loadingScreen.SetActive(true);
            loadingScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        // WaitForSecond have to be longer than the rumbling duration to avoid endless rumbling
        yield return new WaitForSeconds(1.5f);
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(gameScene);
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
