using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    
    [SerializeField] private List<Horde> hordes;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform indicatorParent;
    [SerializeField] private float timeBeforeSpawnEnemies;
    [SerializeField] private GameObject BarrierParent;
    [SerializeField] private ArenaEnter arenaEnter;
    private List<GameObject> indicators;
    private List<EnnemyBase> currentEnnemiesInHordes = new List<EnnemyBase>();
    private int currentHordes = 0;

    private void OnEnable()
    {
        arenaEnter.StartArena += StartArenaFight;
        PlayerController.OnPlayerDeath += CancelArenaFight;
        
    }

    private void OnDisable()
    {
        arenaEnter.StartArena -= StartArenaFight;
        PlayerController.OnPlayerDeath -= CancelArenaFight;
    }

    void Start()
    {
        int maxNumberOfIndicators = 0;
        currentHordes = 0;
        indicators = new List<GameObject>();
        currentEnnemiesInHordes = new List<EnnemyBase>();
        BarrierParent.SetActive(false);
        foreach (Horde horde in hordes)
        {
            if (maxNumberOfIndicators < horde.Enemies.Count)
            {
                maxNumberOfIndicators = horde.Enemies.Count;
            }
        }

        for (int i = 0; i < maxNumberOfIndicators; i++)
        {
            GameObject indicator = Instantiate(indicatorPrefab, indicatorParent);
            indicators.Add(indicator);
            indicators[i].SetActive(false);
        }
    }

    private void StartArenaFight()
    {
        currentEnnemiesInHordes = new List<EnnemyBase>();
        currentHordes = 0;
        BarrierParent.SetActive(true);
        StartCoroutine(StartHordeEnumerator());
    }
    IEnumerator StartHordeEnumerator()
    {
        Horde currentHorde = hordes[currentHordes];
        currentEnnemiesInHordes.Clear();
        for (int i = 0; i < currentHorde.Enemies.Count; i++)
        {
            indicators[i].transform.position = currentHorde.Enemies[i].SpawnPoint;
            indicators[i].SetActive(true);
        }
        yield return new WaitForSeconds(timeBeforeSpawnEnemies);
        for (int i = 0; i < currentHorde.Enemies.Count; i++)
        {
            indicators[i].SetActive(false);
            EnnemyBase currentEnnemy = Instantiate(currentHorde.Enemies[i].Enemy,currentHorde.Enemies[i].SpawnPoint,Quaternion.identity);
            currentEnnemy.alwaysAgro = true;
            currentEnnemy.OnDeath += OnEnemyDeath;
            currentEnnemiesInHordes.Add(currentEnnemy);
        }
    }

    private void OnEnemyDeath(EnnemyBase ennemy)
    {
        if (currentEnnemiesInHordes.Contains(ennemy))
        {
            currentEnnemiesInHordes.Remove(ennemy);
            CheckIfHordeIsFinished();
            ennemy.OnDeath -= OnEnemyDeath;
        }
    }

    private void CancelArenaFight()
    {
        foreach (EnnemyBase enemy in currentEnnemiesInHordes)
        {
            Destroy(enemy.gameObject);
        }
        currentEnnemiesInHordes.Clear();
        currentHordes = 0;
        BarrierParent.SetActive(false);
        arenaEnter.ArenaIsStarted = false;
    }

    private void CheckIfHordeIsFinished()
    {
        if (currentEnnemiesInHordes.Count <= 0)
        {
            currentHordes++;
            CheckIfArenaIsFinished();
        }
    }

    private void CheckIfArenaIsFinished()
    {
        if (currentHordes >= hordes.Count)
        {
            BarrierParent.SetActive(false);
        }
        else
        {
            StartCoroutine(StartHordeEnumerator());
        }
    }
}

[Serializable]
public class Horde
{
    [Serializable]
    public class EnemyInHorde
    {
        public EnnemyBase Enemy;
        public Vector3 SpawnPoint;
    }
    public List<EnemyInHorde> Enemies;
}


