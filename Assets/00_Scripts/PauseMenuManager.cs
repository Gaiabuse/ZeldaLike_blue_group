using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    
    private enum MenuState { Playing, Pause, Settings }
    private MenuState currentState = MenuState.Playing;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }
    
    void Update()
    {
        if (Gamepad.current == null){return;}
        
        if (Gamepad.current.selectButton.wasPressedThisFrame)
        {
            switch (currentState)
            {
                case MenuState.Playing:
                    OpenPauseMenu();
                    break;
                
                case MenuState.Pause:
                    ClosePauseMenu();
                    break;
            }
        }
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1;
        pauseMenu.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() =>
        {
            pauseMenu.SetActive(false); 
        });
        currentState = MenuState.Playing;
    }

    private void OpenPauseMenu()
    {
        currentState = MenuState.Pause;
        pauseMenu.SetActive(true);
        pauseMenu.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).OnComplete(() =>
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
        Debug.Log("restart /not implemented");
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
}
