using System;
using System.Collections.Generic;
using UnityEngine;

public class AttractionZone : MonoBehaviour
{
    [SerializeField] private float AttractionForce = 10f;

    [SerializeField] private List<Ennemy> EnemyAttract;
    

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
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
                        enemy.canMove = false;
                        EnemyAttract.Add(enemy);
                    }
                    
                }
                Vector3 direction = transform.position - other.transform.position;
                Debug.Log(other.name + "se dirige vers " + direction);
                rb.AddForce(direction * AttractionForce * Time.deltaTime, ForceMode.Impulse);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Ennemy ennemy in EnemyAttract)
        {
            ennemy.canMove = true;
            Debug.Log(ennemy.canMove);
        }
        EnemyAttract.Clear();
    }
}
