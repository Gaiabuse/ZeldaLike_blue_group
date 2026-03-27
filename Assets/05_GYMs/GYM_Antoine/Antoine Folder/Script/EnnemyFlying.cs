using UnityEngine;

public class EnnemyFlying : Ennemy
{
    protected override void FixedUpdate()
    {
        float distPlayer = Vector3.Distance(transform.position, Player.position);

        if (distPlayer <= LookRange)
        {
            
        }
    }
}
