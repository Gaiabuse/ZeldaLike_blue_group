using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PauseMenuManager pauseMenu;
    [SerializeField] ProgressMenuUI progressMenu;
    [SerializeField] DebugScreen debugMenu;
    [SerializeField] private bool isTutoActionDone = false;
    [SerializeField] private TutoIndicatorBlink tutoIndicator;

    private bool _isPauseInitialized = false;
    private bool _isProgressInitialized = false;

    public const string PLAYER_INPUT_MAP = "PlayerControl";
    public const string MENU_INPUT_MAP = "MenuControl";
    public const string PROGRESS_INPUT_MAP = "ProgressControl";

    private void Start()
    {
        foreach (InputActionMap actionMap in playerInput.actions.actionMaps)
        {
            actionMap.Disable();
        }
        playerInput.actions.FindActionMap(PLAYER_INPUT_MAP).Enable();
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
        playerInput.SwitchCurrentActionMap(MENU_INPUT_MAP);
        playerInput.actions.FindActionMap(PLAYER_INPUT_MAP).Disable();
    }

    public void OnOpenPhone(InputValue value)
    {
        if (!value.isPressed) return;
        if (!isTutoActionDone)
        {
            if (tutoIndicator == null) return;
            tutoIndicator.StopBlink();
        }
        progressMenu.OpenProgressMenu();
        playerInput.SwitchCurrentActionMap(PROGRESS_INPUT_MAP);
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

    public void OnDebugInput(InputValue value)
    {
        debugMenu.OnActionDebugKey();
    }

    public void OnDebugVisual(InputValue value)
    {
        debugMenu.OnVisualDebugKey();
    }

    private float scrollInput;

    public void OnPhoneScroll(InputValue value)
    {
        scrollInput = value.Get<float>();
    }

    private void Update()
    {
        if (playerInput.currentActionMap.name == PROGRESS_INPUT_MAP && Mathf.Abs(scrollInput) > 0.01f)
        {
            MusicManager.Instance.PlayScroll();
            progressMenu.Scroll(scrollInput);
        }
        else
        {
            MusicManager.Instance.StopScroll();
        }
    }
}
