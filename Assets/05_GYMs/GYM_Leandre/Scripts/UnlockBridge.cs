using UnityEngine;

public class UnlockBridge : MonoBehaviour
{
    [SerializeField] private int numberInteractionNeed = 1;

    public int currentInteraction = 0;
    private bool isBridgeUnlock = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentInteraction = 0;
        gameObject.SetActive(false);
        isBridgeUnlock = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void BridgeUnlock()
    {
        gameObject.SetActive(true);
        isBridgeUnlock = true;
    }

    private void Bridgelock()
    {
        gameObject.SetActive(false);
        isBridgeUnlock = false;
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
        if(currentInteraction > numberInteractionNeed) currentInteraction = numberInteractionNeed;
        CheckInteraction();
    }

    public void RemoveInteraction()
    {
        currentInteraction--;
        if(currentInteraction < 0) currentInteraction = 0;
        CheckInteraction();
    }
}
