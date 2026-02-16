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

    public float damage{private set; get;}
    public TypeOfAttack type{private set; get;}

    public Action Finished;

    public void SetAttack(float damage, TypeOfAttack type)
    {
        this.type = type;
        this.damage = damage;
    }
    
    public void FinishAttack()
    {
        Finished?.Invoke();
        Destroy(gameObject);
    }

}
