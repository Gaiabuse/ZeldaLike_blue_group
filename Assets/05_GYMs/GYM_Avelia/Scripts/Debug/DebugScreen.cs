using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DebugScreen : MonoBehaviour
{
    PlayerPowder powder;
    PlayerHP hp;
    PlayerInput input;

    [Header("InternalReference")]
    [SerializeField]
    GameObject debugAction;
    [SerializeField]
    GameObject debugVisual;

    [SerializeField]
    TMPro.TMP_Text playerPosition;

    [SerializeField]
    GameObject firstobjectdebug;

    void Start()
    {
        GetRef();
    }

    private void GetRef()
    {
        powder = FindAnyObjectByType<PlayerPowder>();
        hp = FindAnyObjectByType<PlayerHP>();
        input = FindAnyObjectByType<PlayerInput>();
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
        EventSystem.current.SetSelectedGameObject(firstobjectdebug);
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

    public void DecreasePowder1() => powder.GainPowder(-1);

    public void IncreasePowder1() => powder.GainPowder(1);

    public void IncreasePowder10() => powder.GainPowder(10);

    public void DecreasePowder10() => powder.GainPowder(-10);

    public void ToggleInfiniteLife(bool value) { hp.invicible = value; }
}

