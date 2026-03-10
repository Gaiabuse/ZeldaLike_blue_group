
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class NeutralAttackManager : AttackManager
{
    [SerializeField]
    private SimpleAttack[] comboAttacks;
    [SerializeField] protected SimpleAttack ChargedAttack;
    [SerializeField]private float rangeOfUltimate;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float durationSleep;
    protected override void OnEnable()
    {
        base.OnEnable();
        numberOfAttacksInCombo = comboAttacks.Length;
    }

    protected override void OnAttack(InputValue _input)
    {
        
        if (_input.isPressed)
        {
            Attack(comboAttacks[currentCombo]);
            return;
        }
        
        if (canChargedAttack)
        {
            canChargedAttack = false;
            Attack(ChargedAttack);
        }
    }

    public override void Ultimate()
    {
        base.Ultimate();
        Debug.Log("Ultimate");
        UltimateActivation();
    }
    private void UltimateActivation()
    {
        Collider[] enmeyHits = Physics.OverlapSphere(transform.position, rangeOfUltimate, enemyLayer);
        foreach (var enemyCollider in enmeyHits)
        {
           Ennemy ennemy = enemyCollider.GetComponent<Ennemy>();
           if (ennemy != null)
           {
               ennemy.StartSleep(durationSleep);
           }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkBlue;
        Gizmos.DrawWireSphere(transform.position, rangeOfUltimate); 
    }
    
}
