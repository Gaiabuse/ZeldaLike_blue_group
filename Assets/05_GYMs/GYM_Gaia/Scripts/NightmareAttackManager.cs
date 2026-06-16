
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
        var attackAction = player.playerInput.actions["Attack"];
        if (attackAction.activeControl != null)
        {
            string direction = attackAction.activeControl.name;

            if (direction != "buttonEast")
            {
                return;
            }
        }
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
            if (CanAttack) player.CanMove = false;
            player.CanRotate = false;
            if (canChargedAttack)
            {
                canChargedAttack = false;
                Attack(ChargedAttack);
                return;
            }
            Attack(comboAttacks[currentCombo]);
            switchInProgress = false;
            
            //FormAnimator.SetBool("isAttacking",true);
            if (FormAnimator.GetBool("isAttacking"))
                FormAnimator.SetTrigger("Attack" + currentCombo);
            Debug.Log("Attack"+currentCombo);
        }
    }


    public override void Ultimate()
    {
        base.Ultimate();
        //UltimateActivation();
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
        ultReference.playerSprite.GetComponent<SpriteRenderer>().enabled = false;
        ultReference.playerCollider.enabled = false;
        ultReference.dash.enabled = false;
        ultReference.grab.enabled = false;
    
        formSwitcher.enabled = false;
    
        ultReference.characterController.detectCollisions = false;
        player.LockRotation = true;
        player.CanMove = true;
    }

    private void UltimateDesactivation()
    {
        formSwitcher.enabled = true; 
        formSwitcher.canSwitchForm = true;
    
        CanAttack = true;
        player.LockRotation = false;
        ultReference.ultimateObject.SetActive(false);
        ultReference.playerCollider.enabled = true;
        ultReference.dash.enabled = true;
        ultReference.grab.enabled = true;
        ultReference.characterController.detectCollisions = true;

        player.CanMove = true;
        player.CanRotate = true;

        Attack(ultimateAttack);
        EndUltimate();
    }

    private IEnumerator UltimateCoroutine()
    {
        player.LockRotation = true;
        FormAnimator.SetTrigger("usingUlti");
    
     
        yield return new WaitForSeconds(0.75f);
        UltimateActivation();
    
        yield return new WaitForSeconds(timeOfUltimate);
        FormAnimator.SetBool("UltiEnd", true);
        ultReference.playerSprite.GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(0.35f);
    
        UltimateDesactivation();
        ultimateCoroutine = null;
    
        yield return new WaitForSeconds(0.5f);
        FormAnimator.SetBool("UltiEnd", false);
    }
}
