using System;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyManager : MonoBehaviour
{
    public List<EnnemyBase> enemies = new List<EnnemyBase>();
    
    [SerializeField] private bool _isInFight;

    public Action OnGameStateChange;
    public static EnnemyManager Instance;

    public bool IsInFight
    {
        get => _isInFight;
        set
        {
            if (_isInFight == value) return; 

            _isInFight = value;
            OnGameStateChange?.Invoke();
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetFightState(bool state)
    {
        IsInFight = state;
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) 
        {
            bool current = _isInFight;
            _isInFight = !current; 
            IsInFight = current;
        }
    }
}