using UnityEngine;

public class SheepEnnemy : Ennemy
{
    [SerializeField] GameObject Shell;
    bool shellHere = true;

    protected override void TakeDamage(int damage)
    {
        if (shellHere)
        {

        }
        else base.TakeDamage(damage);
    }
}
