using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    public Vector3 speed;

    [SerializeField] private LayerMask layerMask;
    void Update()
    {
        transform.transform.position += speed *Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
    }
}
