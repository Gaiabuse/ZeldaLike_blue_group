using System;
using System.Collections.Generic;
using UnityEngine;

public class AttractionZone : MonoBehaviour
{
    [SerializeField] private float AttractionForce = 10f;

    public List<EnnemyBase> EnemyAttract;
    
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private int damagesPerSeconds = 5;
    private float t = 0;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ennemy"))
        {
            EnemyAttract.Add(other.gameObject.GetComponent<EnnemyBase>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ennemy"))
        {
            EnemyAttract.Remove(other.gameObject.GetComponent<EnnemyBase>());
        }
    }

    private void FixedUpdate()
    {
        t  += Time.fixedDeltaTime;
        if (t > 0.5)
        {
            t = 0;
            foreach (EnnemyBase nmi in EnemyAttract)
            {
                nmi.TakeDamage(damagesPerSeconds, 0);
            }
        }
    }

    private void OnDisable()
    {
        EnemyAttract.Clear();
    }
}
