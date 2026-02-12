using UnityEngine;

public class Attack : MonoBehaviour
{
    public enum TypeOfAttack
    {
        Basic,
        nightmare,
        dream
    }

    private float damage;
    private TypeOfAttack type;

    public void SetAttack(float damage, TypeOfAttack type)
    {
        this.type = type;
        this.damage = damage;
    }

    public (float damage,TypeOfAttack type) GetParameters()
    {
        return (damage, type);
    }

}
