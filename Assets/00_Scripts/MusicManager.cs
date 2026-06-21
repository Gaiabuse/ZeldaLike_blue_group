using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private DreamCoreManager dreamCoreManager;
    [SerializeField] private StudioEventEmitter fightMusic;
    [SerializeField] private GameObject bossMusic;
    [SerializeField] private GameObject fightTrigger;
    [SerializeField] private GameObject exploTrigger;
    [SerializeField] private GameObject transfo;
    [SerializeField] private GameObject create;
    [SerializeField] private GameObject erase;
    [SerializeField] private GameObject stun;
    [SerializeField] private GameObject lowLife;
    [SerializeField] private GameObject coreRoar;
    [SerializeField] private GameObject phoneNotification;
    [SerializeField] private GameObject click;
    [SerializeField] private GameObject cancel;
    [SerializeField] private GameObject scroll;
    [SerializeField] private GameObject locked;
    [SerializeField] private GameObject NWalk;
    [SerializeField] private GameObject NWalkBridge;
    [SerializeField] private GameObject NMWalk;
    [SerializeField] private GameObject NMWalkBridge;
    [SerializeField] private GameObject DWalk;
    [SerializeField] private GameObject DShoot1;
    [SerializeField] private GameObject DShoot2;
    [SerializeField] private GameObject Dash;
    [SerializeField] private GameObject arenaStart;
    [SerializeField] private List<GameObject> zonyrHit;
    [SerializeField] private GameObject touchCore;
    
    private PlayerController player;
    private int nbStun;

    public static MusicManager Instance;

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
    
    private void OnDisable()
    {
        if (EnnemyManager.Instance != null)
        {
            EnnemyManager.Instance.OnGameStateChange -= UpdateTriggers;
        }
    }

    private void Start()
    {
        if (EnnemyManager.Instance != null)
        {
            EnnemyManager.Instance.OnGameStateChange += UpdateTriggers;
        }
        else
        {
            Debug.LogError("MusicManager: EnnemyManager.Instance is missing in Start!");
        }
        
        player = FindFirstObjectByType<PlayerController>();
        
        UpdateTriggers();
    }
    
    private void UpdateTriggers()
    {
        if (fightTrigger != null && exploTrigger != null)
        {
            if (dreamCoreManager.isBossActive) return;
            fightTrigger.SetActive(EnnemyManager.Instance.IsInFight);
            exploTrigger.SetActive(!EnnemyManager.Instance.IsInFight);
        }
    }

    public void StartBossMusic()
    {
        fightMusic.enabled = false;
        bossMusic.SetActive(true);
    }
    
    public void StopBossMusic()
    {
        fightMusic.enabled = true;
        bossMusic.SetActive(false);
    }
    
    public void PlayCreate()
    {
        create.SetActive(false);
        create.SetActive(true);
    }

    public void PlayErase()
    {
        erase.SetActive(false);
        erase.SetActive(true);
    }
    
    public void PlayLowLife()
    {
        lowLife.SetActive(true);
    }
    
    public void StopLowLife()
    {
        lowLife.SetActive(false);
    }

    public void RingPhone()
    {
        phoneNotification.SetActive(false);
        phoneNotification.SetActive(true);
    }

    public void PlayClick()
    {
        click.SetActive(false);
        click.SetActive(true);
    }

    public void PlayCancel()
    {
        cancel.SetActive(false);
        cancel.SetActive(true);
    }
    
    public void PlayLockedUI()
    {
        locked.SetActive(false);
        locked.SetActive(true);
    }
    
    public void PlaySwitchForm()
    {
        transfo.SetActive(false);
        transfo.SetActive(true);
    }

    public void PlayZonyrHit()
    {
        GameObject audio = zonyrHit[Random.Range(0, zonyrHit.Count)];
        audio.SetActive(false);
        audio.SetActive(true);
    }

    public void Walk(Form currentForm)
    {
        switch (currentForm)
        {
            case Form.neutral:
                NMWalk.SetActive(false);
                DWalk.SetActive(false);
                NMWalkBridge.SetActive(false);
                if (player.IsOnBridge)
                {
                    NWalkBridge.SetActive(true);
                }
                else NWalk.SetActive(true);
                break;
            case Form.nightmare:
                DWalk.SetActive(false);
                NWalk.SetActive(false);
                NWalkBridge.SetActive(false);
                if (player.IsOnBridge)
                {
                    NMWalkBridge.SetActive(true);
                }
                else NMWalk.SetActive(true);
                break;
            case Form.dream:
                NWalk.SetActive(false);
                NMWalk.SetActive(false);
                DWalk.SetActive(true);
                NWalkBridge.SetActive(false);
                NMWalkBridge.SetActive(false);
                break;
        }
    }

    public void StopWalk()
    {
        NWalk.SetActive(false);
        NMWalk.SetActive(false);
        DWalk.SetActive(false);
        NWalkBridge.SetActive(false);
        NMWalkBridge.SetActive(false);
    }
    
    public void PlayStun()
    {
        nbStun++;
        stun.SetActive(true);
    }
    
    public void PlayCoreRoar()
    {
        coreRoar.SetActive(false);
        coreRoar.SetActive(true);
    }
    
    public void StopStun()
    {
        nbStun--;
        if (nbStun <= 0)
        {
            stun.SetActive(false);
            nbStun = 0;
        }
    }

    public void PlayScroll()
    {
        scroll.SetActive(true);
    }

    public void StopScroll()
    {
        scroll.SetActive(false);
    }

    public void PlayShoot1()
    {
        DShoot1.SetActive(false);
        DShoot1.SetActive(true);
    }
    
    public void PlayShoot2()
    {
        DShoot2.SetActive(false);
        DShoot2.SetActive(true);
    }

    public void PlayDash()
    {
        Dash.SetActive(false);
        Dash.SetActive(true);
    }

    public void PlayArenaStart()
    {
        arenaStart.SetActive(false);
        arenaStart.SetActive(true);
    }

    public void PlayTouchCore()
    {
        touchCore.SetActive(false);
        touchCore.SetActive(true);
    }
}