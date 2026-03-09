using UnityEngine;

public class ExplosionAttack : Attack
{
    [SerializeField]
    private float ManaUsed, Damage;
    [SerializeField]
    private TypeOfAttack Type;

    void start()
    {
        SetAttack(ManaUsed, Damage, null);
    }

}
