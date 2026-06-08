using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    PlayerPowder powder;
    PlayerHP hp;
    PlayerInput input;
    FormSwitcher formSwitcher;
    ErasedManager erasedManager;
    List<Form> choosenForms = new List<Form>() {Form.neutral};

    [Header("InternalReference")]
    [SerializeField]
    GameObject debugAction;
    [SerializeField]
    GameObject debugVisual;

    [SerializeField]
    TMPro.TMP_Text playerPosition;
    
    //[SerializeField] GameObject firstobjectdebug;

    public static DebugScreen Instance;
    
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
    }
    
    void Start()
    {
        GetRef();
    }

    private void GetRef()
    {
        powder = FindAnyObjectByType<PlayerPowder>();
        hp = FindAnyObjectByType<PlayerHP>();
        input = FindAnyObjectByType<PlayerInput>();
        formSwitcher = FindAnyObjectByType<FormSwitcher>();
        erasedManager = FindAnyObjectByType<ErasedManager>();
    }

    public void OnActionDebugKey()
    {
        if (debugAction.activeSelf)
        {
            debugAction.SetActive(false);
            input.SwitchCurrentActionMap(InputManager.PLAYER_INPUT_MAP);
            return;
        }

        debugAction.SetActive(true);
        //EventSystem.current.SetSelectedGameObject(firstobjectdebug);
    }

    public void OnVisualDebugKey()
    {
        if (debugVisual.activeSelf)
        {
            debugVisual.SetActive(false);
            return;
        }

        debugVisual.SetActive(true);
    }

    private void SetForms(Form form, Toggle toggle)
    {
        if (choosenForms.Contains(form))
        {
            if (choosenForms.Count > 1)
            {
                choosenForms.Remove(form);
            }
            else
            {
                toggle.isOn = true;
            }
        }
        else choosenForms.Add(form);
        
        
        formSwitcher.AvailableForms = choosenForms;
        
        if (choosenForms.Count == 1)
        {
            formSwitcher.ChangeForm(choosenForms[0]);
        }
    }

    public void ToggleNeutralForm(Toggle toggle) => SetForms(Form.neutral, toggle);
    public void ToggleNightmareForm(Toggle toggle) => SetForms(Form.nightmare, toggle);
    public void ToggleDreamForm(Toggle toggle) => SetForms(Form.dream, toggle);

    //public void DecreasePowder1() => powder.GainPowder(-1);
    //public void IncreasePowder1() => powder.GainPowder(1);

    public void IncreasePowder10() => powder.GainPowder(10);
    public void DecreasePowder10() => powder.GainPowder(-10);
    
    public void GainCPoint() => erasedManager.GainPointForCreate();
    public void LooseCPoint() => erasedManager.LoosePointForCreate();

    public void ToggleInfiniteLife(Toggle toggle) { hp.invicible = toggle.isOn; }

    public void Activate()
    {
        debugVisual.SetActive(true);
        debugAction.SetActive(true);
    }

    public void Disactivate()
    {
        debugVisual.SetActive(false);
        debugAction.SetActive(false);
    }
}

