using System;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private GameObject fightTrigger;
    [SerializeField] private GameObject exploTrigger;
    [SerializeField] private GameObject transfo;
    [SerializeField] private GameObject create;
    [SerializeField] private GameObject erase;
    [SerializeField] private GameObject stun;
    [SerializeField] private GameObject phoneNotification;
    [SerializeField] private GameObject click;
    [SerializeField] private GameObject cancel;
    [SerializeField] private GameObject scroll;
    [SerializeField] private GameObject locked;
    [SerializeField] private GameObject NWalk;
    [SerializeField] private GameObject NMWalk;
    [SerializeField] private GameObject DWalk;
    
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

    public void Walk(Form currentForm)
    {
        switch (currentForm)
        {
            case Form.neutral:
                NMWalk.SetActive(false);
                DWalk.SetActive(false);
                NWalk.SetActive(true);
                break;
            case Form.nightmare:
                DWalk.SetActive(false);
                NWalk.SetActive(false);
                NMWalk.SetActive(true);
                break;
            case Form.dream:
                NWalk.SetActive(false);
                NMWalk.SetActive(false);
                DWalk.SetActive(true);
                break;
        }
    }

    public void StopWalk()
    {
        NWalk.SetActive(false);
        NMWalk.SetActive(false);
        DWalk.SetActive(false);
    }
    
    public void PlayStun()
    {
        stun.SetActive(true);
    }
    
    public void StopStun()
    {
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
}