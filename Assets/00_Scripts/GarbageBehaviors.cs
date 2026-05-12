using System;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class GarbageBehaviors : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float spawnRatio;
    [SerializeField] private int containPowder;
    [SerializeField] [Range(0,100)] private int cleanPoints;
    [SerializeField] [Range(0,2)] private int cleanPointsPLevel;
    [SerializeField] int hp = 1;
    [SerializeField] private bool isGlue;
    private int _hp;
    
    private GameObject player;
    

    private void Start()
    {
        _hp = 2 * hp;
        player = GameObject.FindGameObjectWithTag("Player");

        if (containPowder > 0)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            int layer = LayerMask.NameToLayer("ErasedObject");
            gameObject.layer = layer;
        }
    }

    public void Clean()
    {
        _hp--;
        if (_hp > 0) return;
        DoClean();
    }

    private void DoClean()
    {

        if (containPowder > 0)
        {
            if (player == null) return;
            player.GetComponent<PlayerPowder>().GainPowder(containPowder);
        }
        
        QuotaManager.Instance.GainCleanPoints(cleanPoints, cleanPointsPLevel);

        if (isGlue)
        {
            if (transform.parent.gameObject == null) return;
            GetComponentInParent<Glue>().CleanGlue();
        }
        Destroy(gameObject);
    }
}
