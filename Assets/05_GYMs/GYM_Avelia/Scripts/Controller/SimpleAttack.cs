using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SimpleAttack : MonoBehaviour
{
    [SerializeField]
    protected AttackData basicAttackData;

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
        if (type == global::Attack.TypeOfAttack.Nightmare)
        {
            if (chargedAttack)
            {
                chargedAttack = false;
                Attack(ChargedAttackData);
            }
            else
            {
                Attack(basicAttackData);
            }
        }
        else
        {
            Attack(basicAttackData);
        }
        
    }

    void OnChargedAttack(InputValue _input)
    {
        Debug.Log("charged Attack");
        chargedAttack = true;
    }

    private void Attack(AttackData data)
    {
        if (!canAttack) return;
        canAttack = false;
        var lAttack = Instantiate(data.attackPrefab,player.transform);
        lAttack.SetAttack(data.damage, type);
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
