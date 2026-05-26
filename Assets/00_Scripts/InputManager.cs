using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PauseMenuManager pauseMenu;
    [SerializeField] ProgressMenuUI progressMenu;
    [SerializeField] IsometricParalaxe paralaxe;
    
    private bool _isPauseInitialized = false;
    private bool _isProgressInitialized = false;

    private void Start()
    {
        foreach (InputActionMap actionMap in playerInput.actions.actionMaps)
        {
            actionMap.Disable();
        }
        playerInput.actions.FindActionMap("PlayerControl").Enable();
    }

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
        playerInput.actions.FindActionMap("PlayerControl").Disable();
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
    
    public void OnSwitchToggle(InputValue value)
    {
        progressMenu.SwitchToggle();
    }

    private float scrollInput;

    public void OnPhoneScroll(InputValue value)
    {
        scrollInput = value.Get<float>();
    }
    
    public void OnMove(InputValue value)
    {
        paralaxe.Move(value.Get<Vector2>());
    }

    private void Update()
    {
        if (playerInput.currentActionMap.name == "ProgressControl" && Mathf.Abs(scrollInput) > 0.01f)
        {
            progressMenu.Scroll(scrollInput);
        }
    }
}