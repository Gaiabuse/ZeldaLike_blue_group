using System;
using UnityEngine;

public class ExplosionCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var damageableOther = other.GetComponent<IPlayerDamageable>();
        if (damageableOther == null) return;

        GetComponent<SphereCollider>().enabled = false;
        GetComponentInParent<StarBomb>().DealDamages(damageableOther);
    }
}
