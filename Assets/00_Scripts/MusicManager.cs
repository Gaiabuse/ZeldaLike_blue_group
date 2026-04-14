using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private GameObject fightTrigger;
    [SerializeField] private GameObject exploTrigger;
    
    [SerializeField] private bool _isInFight = false;

    public bool isInFight
    {
        get => _isInFight;
        set
        {
            _isInFight = value;
            UpdateTriggers();
        }
    }

    private void Start()
    {
        UpdateTriggers();
    }
    
    private void UpdateTriggers()
    {
        if (fightTrigger != null && exploTrigger != null)
        {
            fightTrigger.SetActive(_isInFight);
            exploTrigger.SetActive(!_isInFight);
        }
    }
    
    private void OnValidate()
    {
        UpdateTriggers();
    }
}