using System;
using System.Collections.Generic;
using UnityEngine;

public class AttractionZone : MonoBehaviour
{
    [SerializeField] private float AttractionForce = 10f;

    [SerializeField] private List<Ennemy> EnemyAttract;
    

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ennemy"))
        {
            Debug.Log(other.name);
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Ennemy enemy  = rb.GetComponent<Ennemy>();
                if (enemy)
                {
                    if (!EnemyAttract.Contains(enemy))
                    {
                        EnemyAttract.Add(enemy);
                    }
                    
                }
                Vector3 direction = transform.position - other.transform.position;
                rb.AddForce(direction * AttractionForce * Time.deltaTime, ForceMode.Impulse);
            }
        }
    }

    private void OnDisable()
    {
        EnemyAttract.Clear();
    }
}
