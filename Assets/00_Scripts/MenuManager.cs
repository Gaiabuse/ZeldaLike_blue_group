using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameScene;
    [SerializeField] private GameObject menuIllu;
    [SerializeField] private GameObject menuCutscene;
    [SerializeField] private GameObject mainFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private GameObject clickSFX;
    [SerializeField] private GameObject cancelSFX;
    [SerializeField] private TMP_Text versionText;
    
    private enum MenuState { Title, Settings, Credits }
    private MenuState currentState = MenuState.Title;
    private GameObject lastFocusedButton;
    
    private void Start()
    {
        SetVersionText(); 
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
        lastFocusedButton = EventSystem.current.currentSelectedGameObject;
        Time.timeScale = 1f;
    }
    
    private void OnValidate()
    {
        SetVersionText();
    }
    
    private void SetVersionText()
    {
        string currentVersion = Application.version; 
        
        if (versionText != null)
        {
            versionText.text = $"v{currentVersion}";
        }
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

    private void Update()
    {
        GameObject currentSelection = EventSystem.current.currentSelectedGameObject;

        if (currentSelection != null)
        {
            // The player is safely navigating; remember this button!
            lastFocusedButton = currentSelection;
        }
        else
        {
            // Focus was lost to a mouse click! Restore the last known button.
            if (lastFocusedButton != null && lastFocusedButton.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastFocusedButton);
            }
        }
    }

    private void RefocusMenu()
    {
        switch (currentState)
        {
            case MenuState.Title:
                EventSystem.current.SetSelectedGameObject(mainFirstSelected);
                break;

            case MenuState.Settings:
                EventSystem.current.SetSelectedGameObject(settingsFirstSelected);
                break;

            case MenuState.Credits:
                break;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Play()
    {
        clickSFX.SetActive(false);
        clickSFX.SetActive(true);
        StartCoroutine(LaunchGameSequence());
    }
    
    public void ShowSettings()
    {
        clickSFX.SetActive(false);
        clickSFX.SetActive(true);
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            settingsScreen.SetActive(true);
        
            EventSystem.current.SetSelectedGameObject(settingsFirstSelected);
            lastFocusedButton = settingsFirstSelected; 
        
            settingsScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
    
        currentState = MenuState.Settings;
    }

    public void CloseSettings()
    {
        cancelSFX.SetActive(false);
        cancelSFX.SetActive(true);
        settingsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            settingsScreen.SetActive(false);
            titleScreen.SetActive(true);
        
            EventSystem.current.SetSelectedGameObject(mainFirstSelected);
            lastFocusedButton = mainFirstSelected;
        
            titleScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
    
        currentState = MenuState.Title;
    }
    
    
    public void ShowCredits()
    {
        clickSFX.SetActive(false);
        clickSFX.SetActive(true);
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            creditsScreen.SetActive(true);
            creditsScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Credits;
    }
    
    public void CloseCredits()
    {
        cancelSFX.SetActive(false);
        cancelSFX.SetActive(true);
        creditsScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            creditsScreen.SetActive(false);
            titleScreen.SetActive(true);
            EventSystem.current.SetSelectedGameObject(mainFirstSelected);
            titleScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        currentState = MenuState.Title;
    }
    
    private IEnumerator LaunchGameSequence()
    {
        StartCoroutine(RumbleCoroutine(0.5f, 0.5f, 0.5f));
        menuIllu.SetActive(false);
        menuCutscene.SetActive(true);

        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f).OnComplete(() => {
            titleScreen.SetActive(false);
            loadingScreen.SetActive(true);
            loadingScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        });
        
        Animator cutsceneAnimator = menuCutscene.GetComponent<Animator>();
        
        yield return new WaitUntil(() => 
            !cutsceneAnimator.IsInTransition(0) 
            && cutsceneAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        SceneManager.LoadSceneAsync(gameScene);
    }
    

    private IEnumerator<WaitForSeconds> RumbleCoroutine(float duration, float low, float high)
    {
        RumbleManager.Instance.TriggerVibration(low, high);
        yield return new WaitForSeconds(duration);
        RumbleManager.Instance.StopVibration();
    }
}
