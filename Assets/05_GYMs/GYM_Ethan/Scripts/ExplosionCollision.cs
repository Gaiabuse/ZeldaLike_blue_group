using System;
using UnityEngine;

public class ExplosionCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<SphereCollider>().enabled = false;
        this.GetComponentInParent<StarBomb>().DealDamages(other.gameObject);
    }
}
