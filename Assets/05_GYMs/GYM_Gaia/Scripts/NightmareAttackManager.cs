
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Object = System.Object;

public class NightmareAttackManager : AttackManager
{
    [SerializeField]
    private SimpleAttack[] comboAttacks;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [Header("Ult")]
    [SerializeField] private float timeOfUltimate;
    [SerializeField] private SimpleAttack ultimateAttack;
    [Tooltip("dont Open please")]
    [SerializeField] private UltReference ultReference;
  
    private Coroutine ultimateCoroutine;

    [Serializable]
    private class UltReference
    {
        public CharacterController characterController;
        public GameObject playerSprite;
        public Collider playerCollider;
        public GameObject ultimateObject;
        public GrabSystem grab;
        public DreamDash dash;
    }
    private void Awake()
    {
        ultReference.ultimateObject.SetActive(false);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
    }

    protected override void OnAttack(InputValue _input)
    {
        base.OnAttack(_input);
        Debug.Log(switchInProgress);
        if (!_input.isPressed && switchInProgress)
        {
            if (finishSwitchCoroutine != null)
            {
                StopCoroutine(finishSwitchCoroutine);
            }
            finishSwitchCoroutine = StartCoroutine(FinishSwitch());
        }
        if (switchInProgress)
        {
            return;
        }
        
        if (!_input.isPressed)
        {
            var targetComponent = AutoAimable.GetNearestTargetAround(transform.position, 30f);
    
            if (targetComponent != null)
            {
                Vector3 targetPos = targetComponent.transform.position;
                targetPos.y = transform.parent.position.y;
                transform.parent.LookAt(targetPos);
            }
            player.CanMove = false;
            player.CanRotate = false;
            if (canChargedAttack)
            {
                canChargedAttack = false;
                Attack(ChargedAttack);
                return;
            }
            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;
        }
    }


    public override void Ultimate()
    {
        base.Ultimate();
        UltimateActivation();
        if (ultimateCoroutine != null)
        {
            StopCoroutine(ultimateCoroutine);
        }
        ultimateCoroutine = StartCoroutine(UltimateCoroutine());
    }
    private void UltimateActivation()
    {
        formSwitcher.canSwitchForm = false;
        CanAttack = false;
        ultReference.ultimateObject.SetActive(true);
        ultReference.playerSprite.SetActive(false);
        ultReference.playerCollider.enabled = false;
        ultReference.dash.enabled = false;
        ultReference.grab.enabled = false;
        formSwitcher.enabled = false;
        ultReference.characterController.detectCollisions = false;
    }

    private void UltimateDesactivation()
    {
        formSwitcher.canSwitchForm = true;
        CanAttack = true;
        ultReference.ultimateObject.SetActive(false);
        ultReference.playerSprite.SetActive(true);
        ultReference.playerCollider.enabled = true;
        ultReference.dash.enabled = true;
        ultReference.grab.enabled = true;
        ultReference.characterController.detectCollisions = true;
        Attack(ultimateAttack);
    }

    private IEnumerator UltimateCoroutine()
    {
        yield return new WaitForSeconds(timeOfUltimate);
        formSwitcher.canSwitchForm = true;
        UltimateDesactivation();
        ultimateCoroutine = null;
    }
}
