using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    
    [Tooltip("chaque element est une horde d'ennemi")]
    [SerializeField] private List<Horde> hordes;
    [Tooltip("prefab de ce qui va apparaitre a la position de l'ennemi avant qu'il spawn")]
    [SerializeField] private GameObject indicatorPrefab;
    [Tooltip("parent des indicateur ")]
    [SerializeField] private Transform indicatorParent;
    [Tooltip("temps que reste l'indicateur avant le spawn de l'ennemie ")]
    [SerializeField] private float timeBeforeSpawnEnemies;
    [Tooltip("parent des barrière qui bloque le joueur dans l'arène")]
    [SerializeField] private GameObject BarrierParent;
    [Tooltip("Detecteur de l'entrée de l'arene qui va lancer le combat")]
    [SerializeField] private ArenaEnter arenaEnter;
    [SerializeField] [Range(0,100)] private int cleanPoints;
    private List<GameObject> indicators;
    private List<EnnemyBase> currentEnnemiesInHordes = new List<EnnemyBase>();
    private int currentHordes = 0;

    private bool ArenaIsFinished = false;
    public static Action StartArena;
    public static Action FinishArena;
    private void OnEnable()
    {
        arenaEnter.StartArena += StartArenaFight;
        PlayerController.OnRespawn += CancelArenaFight;
        
    }

    private void OnDisable()
    {
        arenaEnter.StartArena -= StartArenaFight;
        PlayerController.OnRespawn -= CancelArenaFight;
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
        StartArena?.Invoke();
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
        if(ArenaIsFinished) return;
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
            Debug.Log("check1");
            currentHordes++;
            CheckIfArenaIsFinished();
        }
    }

    private void CheckIfArenaIsFinished()
    {
        Debug.Log("check2 : " +currentHordes +"/"+ hordes.Count);
        if (currentHordes >= hordes.Count)
        {
            BarrierParent.SetActive(false);
            // fx clean
            QuotaManager.Instance.GainCleanPoints(cleanPoints);
            FinishArena?.Invoke();
            ArenaIsFinished = true;
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


