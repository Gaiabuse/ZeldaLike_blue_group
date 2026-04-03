using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PauseMenuManager pauseMenu;
    
    private bool _isRealeased = false;
    private bool _canUnpause = false;

    public void OnPause(InputValue value)
    {
        _isRealeased = !value.isPressed;
        
        if (value.isPressed)
        {
            pauseMenu.OpenPauseMenu();
            playerInput.SwitchCurrentActionMap("MenuControl");
            StartCoroutine(WaitForInputRelease(value));
        }
    }

    public void OnUnpause(InputValue value)
    {
        _isRealeased = !value.isPressed;
        if (!_canUnpause){return;}
        
        if (value.isPressed)
        {
            pauseMenu.ClosePauseMenu();
            playerInput.SwitchCurrentActionMap("PlayerControl");
        }
    }

    public void OnReturn(InputValue value)
    {
        pauseMenu.Return();
    }

    private IEnumerator WaitForInputRelease(InputValue value)
    {
        _canUnpause = false;
        yield return new WaitUntil(()=>_isRealeased == true);
        _canUnpause = true;
    }
}
