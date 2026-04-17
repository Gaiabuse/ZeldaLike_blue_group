using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private GameObject fightTrigger;
    [SerializeField] private GameObject exploTrigger;


    private void OnEnable()
    {
        EnnemyManager.Instance.OnGameStateChange += UpdateTriggers;
    }
    
    private void OnDisable()
    {
        EnnemyManager.Instance.OnGameStateChange -= UpdateTriggers;
    }

    private void Start()
    {
        UpdateTriggers();
    }
    
    private void UpdateTriggers()
    {
        if (fightTrigger != null && exploTrigger != null)
        {
            fightTrigger.SetActive(EnnemyManager.Instance.IsInFight);
            exploTrigger.SetActive(!EnnemyManager.Instance.IsInFight);
        }
    }
}