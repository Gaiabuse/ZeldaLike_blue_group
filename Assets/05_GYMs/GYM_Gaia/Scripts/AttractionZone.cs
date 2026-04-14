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
            Vector3 direction = transform.position - other.transform.position;
            other.transform.Translate(direction.normalized * AttractionForce * Time.deltaTime, Space.World);
            
        }
    }

    private void OnDisable()
    {
        EnemyAttract.Clear();
    }
}
