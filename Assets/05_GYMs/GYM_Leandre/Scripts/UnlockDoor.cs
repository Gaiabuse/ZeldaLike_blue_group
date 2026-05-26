using UnityEngine;

public class UnlockDoor : MonoBehaviour
{
    [SerializeField] private int numberInteractionNeed = 1;

    public int currentInteraction = 0;
    private bool isDoorUnlock = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentInteraction = 0;
        gameObject.SetActive(true);
        isDoorUnlock = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void BridgeUnlock()
    {
        gameObject.SetActive(false);
        isDoorUnlock = false;
    }

    private void Bridgelock()
    {
        gameObject.SetActive(true);
        isDoorUnlock = true;
    }
    
    private void CheckInteraction()
    {
        if (currentInteraction >= numberInteractionNeed)
        {
            BridgeUnlock();
        }
        else
        {
            Bridgelock();
        }
    }

    public void AddInteraction()
    {
        currentInteraction++;
        CheckInteraction();
    }

    public void RemoveInteraction()
    {
        currentInteraction--;
        CheckInteraction();
    }
}
