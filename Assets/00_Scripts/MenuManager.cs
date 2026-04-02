using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameScene;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadingScreen;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
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
        Debug.Log("ShowSettings");
    }
    
    public void ShowCredits()
    {
        Debug.Log("ShowCredits");
    }
    
    private IEnumerator LaunchGameSequence()
    {
        StartCoroutine(RumbleCoroutine(0.5f, 0.5f, 0.5f));
        
        titleScreen.GetComponent<CanvasGroup>().DOFade(0f, 0.25f);
        titleScreen.SetActive(false);
        loadingScreen.SetActive(true);
        loadingScreen.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        
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
