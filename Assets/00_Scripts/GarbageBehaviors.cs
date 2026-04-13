using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GarbageBehaviors : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float spawnRatio;
    [SerializeField] private bool hasZonyr;
    [SerializeField] private GameObject zonyr;
    
    private int _hp = 1;

    private void Start()
    {
        if (!hasZonyr)
        {
            if (Random.Range(0f, 1f) <= spawnRatio)
            {
                hasZonyr = true;
            }
            
        }
    }

    public void TakeDamage(int damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
            if (hasZonyr)
            {
                Instantiate(zonyr, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
    }
}
