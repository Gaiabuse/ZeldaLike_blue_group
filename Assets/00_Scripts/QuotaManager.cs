using System;
using UnityEngine;

public class QuotaManager : MonoBehaviour
{
    [Range(0,100)] public int cleanPoints;
    [Range(0,100)] public int[] quotas = new int[3];
    [SerializeField] private int bonusPoints;
    [SerializeField] private ErasedManager player;
    [SerializeField] private ProgressMenuUI progressMenuUI;
    
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

    public void GainCleanPoints(int progress, int level)
    {
        if (level > quotaIndex)
        {
            GainBonusPoints(progress);
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
        if (cleanPoints >= quotas[quotaIndex])
        {
            GainBonusPoints(cleanPoints - quotas[quotaIndex]); //Add bonus points if overflow
            cleanPoints = quotas[quotaIndex];
            quotaIndex++;
            player.GainPointForCreate();
        }
    }
}
