using System;
using UnityEngine;

public class QuotaManager : MonoBehaviour
{
    [Range(0,100)] public int cleanPoints;
    [Range(0,100)] public int[] quotas = new int[3];
    [SerializeField] private int bonusPoints;
    [SerializeField] private ErasedManager player;
    [SerializeField] private ProgressMenuUI progressMenuUI;
    public int DustCount;
    public int cleanedDustCount;
    
    private int quotaIndex;
    
    public static QuotaManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GainCleanPoints(int progress)
    {
        if (cleanPoints+progress > 100)
        {
            cleanPoints = 100;
        }
        else
        {
            cleanPoints += progress;
            CheckQuota();
            progressMenuUI.ShowProgressPopUp();
        }
    }
    
    public void GainBonusPoints(int points)
    {
        bonusPoints += points;
        //TODO call for update visuel
    }

    private void CheckQuota()
    {
        if (quotaIndex >= quotas.Length) return;
        if (cleanPoints >= quotas[quotaIndex])
        {
            quotaIndex++;
            player.GainPointForCreate();
        }
    }

    public void DustCleaned()
    {
        cleanedDustCount++;
    }
}
