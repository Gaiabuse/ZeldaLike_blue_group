using System;
using System.Collections;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class GarbageBehaviors : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float spawnRatio;
    [SerializeField] private int containPowder;
    [SerializeField] [Range(0,100)] private int cleanPoints;
    [SerializeField] int hp = 1;
    [SerializeField] private bool isGlue;
    [SerializeField] private VisualEffect hitVFX;
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
                    if (child.transform.childCount > 1)
                    {
                        child.GetChild(0).gameObject.SetActive(true);
                        child.GetChild(1).gameObject.SetActive(true); 
                    }
                    
                }
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    public void Clean()
    {
        if (!isCleaning)
        {
            if (!isGlue)
            {
                PlayVFX();
            }
            isCleaning = true;
            hp--;
            MusicManager.Instance.PlayGooHit();
            StartCoroutine(CleanPause());
        }
    }

    private void PlayVFX()
    {
        hitVFX.transform.SetParent(transform.parent);
        hitVFX.transform.position = transform.position;
        Vector3 lookTarget = new Vector3(player.transform.position.x, hitVFX.transform.position.y, player.transform.position.z);
        hitVFX.transform.LookAt(lookTarget);
        hitVFX.transform.Rotate(0, 90, 0);

        hitVFX.enabled = true;
        hitVFX.Play();
    }

    IEnumerator CleanPause()
    {
        yield return new WaitForSeconds(0.25f);
        isCleaning = false;
        if (hp > 0) yield break;
        DoClean(); 
    }

    private void DoClean()
    {
        if (containPowder > 0)
        {
            if (player == null) return;
            player.GetComponent<PlayerPowder>().GainPowder(containPowder);
        }
        
        QuotaManager.Instance.GainCleanPoints(cleanPoints);
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

    public void PlayGlueVFX(Transform collisionTransform)
    {
        hitVFX.transform.SetParent(collisionTransform);
        hitVFX.transform.position = collisionTransform.position;
        Vector3 lookTarget = new Vector3(player.transform.position.x, hitVFX.transform.position.y, player.transform.position.z);
        hitVFX.transform.LookAt(lookTarget);
        hitVFX.transform.Rotate(0, 90, 0);

        hitVFX.enabled = true;
        hitVFX.Play();
    }
}
