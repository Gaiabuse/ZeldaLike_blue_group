using System;
using System.Collections;
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
    private bool isCleaning = false;
    
    private GameObject player;
    

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        QuotaManager.Instance.DustCount++;
        
        if (containPowder > 0)
        {
            if (isGlue)
            {
                foreach (Transform child in transform)
                {
                    child.GetChild(0).gameObject.SetActive(true);
                    child.GetChild(1).gameObject.SetActive(true);
                }
            }
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void Clean()
    {
        if (!isCleaning)
        {
            isCleaning = true;
            hp--;
            Debug.Log(hp);
            StartCoroutine(CleanPause());
            if (hp > 0) return;
            DoClean(); 
        }
    }

    IEnumerator CleanPause()
    {
        yield return new WaitForSeconds(0.25f);
        isCleaning = false;
    }

    private void DoClean()
    {
        if (containPowder > 0)
        {
            if (player == null) return;
            player.GetComponent<PlayerPowder>().GainPowder(containPowder);
        }
        
        QuotaManager.Instance.GainCleanPoints(cleanPoints, cleanPointsPLevel);
        QuotaManager.Instance.DustCleaned();

        if (isGlue)
        {
            GetComponent<Glue>().CleanGlue();
            enabled = false;
        }
        else
        {
            Destroy(gameObject); 
        }
    }
}
