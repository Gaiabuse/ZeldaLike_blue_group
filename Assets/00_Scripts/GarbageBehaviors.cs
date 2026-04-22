using System;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class GarbageBehaviors : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float spawnRatio;
    [SerializeField] private int containPowder;
    [SerializeField] private bool hasZonyr;
    [SerializeField] private GameObject zonyr;
    [SerializeField] [Range(0,100)] private int cleanPoints;
    [SerializeField] [Range(0,2)] private int cleanPointsPLevel;
    
    private GameObject player;
    
    private int _hp = 1;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (containPowder > 0)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            int layer = LayerMask.NameToLayer("ErasedObject");
            gameObject.layer = layer;
        }
        
        if (!hasZonyr)
        {
            if (Random.Range(0f, 1f) <= spawnRatio)
            {
                hasZonyr = true;
            }
            
        }
    }


    public void Clean()
    {
        if (containPowder > 0) return;
        DoClean();
    }
    
    public void Erase()
    {
        DoClean();
    }

    private void DoClean()
    {
        if (containPowder > 0)
        {
            if (player == null) return;
            player.GetComponent<PlayerPowder>().GainPowder(containPowder);
        }
        
        if (hasZonyr)
        {
            Instantiate(zonyr, transform.position, transform.rotation);
        }
        
        QuotaManager.Instance.GainCleanPoints(cleanPoints, cleanPointsPLevel);
        Destroy(gameObject);
    }
}
