using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowMode : MonoBehaviour
{
    [SerializeField] private GameObject shadow;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private AttackData attackData;
    [SerializeField] private SimpleAttack shadowAttack;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private GameObject[] playerObjects;
    [SerializeField] private LayerMask shadowMask;
    private bool isActiveShadow = false;


    [SerializeField]private bool stayOnClick = false;
    private void OnEnable()
    {
        shadow.SetActive(false);
    }

    private void OnDisable()
    {
        ShadowModeForceDesactivation();
    }

    private void ShadowModeForceDesactivation()
    {
        ShadowModeActivation(false,true);
    }
    private void ShadowModeActivation(bool isActive,bool force = false)
    {
        shadow.SetActive(isActive);
        playerCollider.enabled = !isActive;
        playerController.detectCollisions = !isActive;
        playerController.excludeLayers = isActive ? shadowMask : new LayerMask();
        foreach (var go in playerObjects)
        {
            go.SetActive(!isActive);
        }

        if (isActive == false && force == false)
        {
            shadowAttack.Attack(attackData);
        }
    }
    

    public void OnDash(InputValue _input)
    {
        if (stayOnClick)
        {
            if (_input.isPressed)
            {
                ShadowModeActivation(true);
                return;
            }
            ShadowModeActivation(false);
            return;
        }
        isActiveShadow = !isActiveShadow;
        ShadowModeActivation(isActiveShadow);
    }
}
