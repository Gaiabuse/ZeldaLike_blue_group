using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SimpleAttack : MonoBehaviour
{
    [SerializeField]
    protected AttackData basicAttackData;

    [SerializeField] private ManaGauge manaGauge;
    [SerializeField] protected AttackData ChargedAttackData;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField]protected PlayerController player;

    private bool canAttack;
    private bool chargedAttack;
    private Attack currentAttack;
    private void OnEnable()
    {
        canAttack = true;
        chargedAttack = false;
    }

  
    void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            Debug.Log("isPressed");
            Attack(basicAttackData);
            return;
        }
        if (type == global::Attack.TypeOfAttack.Nightmare)
        {
            if (chargedAttack)
            {
                chargedAttack = false;
                Attack(ChargedAttackData);
            }
        }
        
    }

    void OnChargedAttack(InputValue _input)
    {
        Debug.Log("charged Attack");
        chargedAttack = true;
    }

    public void Attack(AttackData data)
    {
        if (!canAttack) return;
        canAttack = false;
        var lAttack = Instantiate(data.attackPrefab,player.transform);
        lAttack.SetAttack(data, type, manaGauge);
        currentAttack = lAttack;
        currentAttack.Finished += AttackIsFinished;
    }

    private void AttackIsFinished()
    {
        if(currentAttack == null)return;
        canAttack = true;
        currentAttack.Finished -= AttackIsFinished;
        currentAttack = null;
    }
}
