using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PauseMenuManager pauseMenu;
    [SerializeField] ProgressMenuUI progressMenu;
    
    private bool _isPauseInitialized = false;
    private bool _isProgressInitialized = false;
    
    public void OnPause(InputValue value)
    {
        if (value.isPressed) 
        {
            DoPause();
        }
    }

    public void OnUnpause(InputValue value)
    { 
        DoUnpause();
    }

    private void DoPause()
    {
        pauseMenu.OpenPauseMenu();
        playerInput.SwitchCurrentActionMap("MenuControl");
    }
    
    public void OnOpenPhone(InputValue value)
    {
        if (!value.isPressed) return; 
        progressMenu.OpenProgressMenu();
        playerInput.SwitchCurrentActionMap("ProgressControl");
    }
    
    private void DoUnpause()
    {
        if (_isPauseInitialized)
        {
            pauseMenu.ClosePauseMenu();
        }
        else
        {
            _isPauseInitialized = true;
        }
    }
    
    public void OnClosePhone(InputValue value)
    {
        if (_isProgressInitialized)
        {
            progressMenu.CloseProgressMenu();
        }
        else
        {
            _isProgressInitialized = true;
        }
    }

    public void OnReturn(InputValue value)
    {
        if (value.isPressed)
            pauseMenu.Return();
    }
}