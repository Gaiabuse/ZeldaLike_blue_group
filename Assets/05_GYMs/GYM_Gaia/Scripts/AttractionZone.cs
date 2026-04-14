using System;
using System.Collections.Generic;
using UnityEngine;

public class AttractionZone : MonoBehaviour
{
    [SerializeField] private float AttractionForce = 10f;

    [SerializeField] private List<Ennemy> EnemyAttract;
    
    [SerializeField] private float stopDistance = 2f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ennemy"))
        {
            Vector3 offset = transform.position - other.transform.position;
            float distance = offset.magnitude;
            

            if (distance > stopDistance)
            {
                Vector3 direction = offset.normalized;
                
                other.transform.Translate(direction * AttractionForce * Time.deltaTime, Space.World);
            }
        }
    }
    
    private void OnDisable()
    {
        EnemyAttract.Clear();
    }
}
