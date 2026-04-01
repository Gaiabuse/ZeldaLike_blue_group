using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
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
        SceneManager.LoadScene("03_Scenes/LD_Playtest");
    }

    public void ShowSettings()
    {
        Debug.Log("ShowSettings");
    }
    
    public void ShowCredits()
    {
        Debug.Log("ShowCredits");
    }
}
