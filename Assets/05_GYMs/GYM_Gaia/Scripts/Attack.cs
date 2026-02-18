using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public enum TypeOfAttack
    {
        Basic,
        Nightmare,
        Dream
    }
    private ManaGauge manaGauge;

    public float manaUsed{private set; get;}
    public float damage{private set; get;}
    public TypeOfAttack type{private set; get;}

    public Action Finished;
    private bool touchedEnemy;

    public void SetAttack(AttackData data, TypeOfAttack type,ManaGauge manaGauge)
    {
        this.type = type;
        this.damage = data.damage;
        manaUsed = data.mana;
        this.manaGauge = manaGauge;
    }
    
    public void FinishAttack()
    {
        if (touchedEnemy)
        {
            if (type == TypeOfAttack.Basic)
            {
                AddMana();
            }
        }
        Finished?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (type == TypeOfAttack.Basic)
            {
                touchedEnemy = true;
            }
                
        }
    }

    private void AddMana()
    {
        manaGauge.AddMana(manaUsed);
    }
}
