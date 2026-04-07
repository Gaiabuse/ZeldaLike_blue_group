using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PauseMenuManager pauseMenu;
    
    private bool _isInitialized = false;
    
    public void OnPause(InputValue value)
    {
        if (value.isPressed) 
        {
            DoPause();
        }
    }

    public void OnUnpause(InputValue value)
    {
        if (value.isPressed)
        {
            DoUnpause();
        }
    }

    private void DoPause()
    {
        pauseMenu.OpenPauseMenu();
        playerInput.SwitchCurrentActionMap("MenuControl");
    }
    
    private void DoUnpause()
    {
        if (_isInitialized)
        {
            pauseMenu.ClosePauseMenu();
            playerInput.SwitchCurrentActionMap("PlayerControl");
        }
        else
        {
            _isInitialized = true;
        }
    }

    public void OnReturn(InputValue value)
    {
        if (value.isPressed)
            pauseMenu.Return();
    }
}