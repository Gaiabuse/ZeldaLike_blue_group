using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;

public class DreamBait : MonoBehaviour
{
    [SerializeField]
    private DreamBaitProps BaitPrefab;

    private DreamBaitProps currentBaitInstance;

    async Task OnSecondPower(InputValue _input)
    {
        if (!_input.isPressed) return;

        if (currentBaitInstance == null)
        {
            DoBaitSpawn();
            return;
        }

        // probably needs another way to do it but this will do it for now
        await currentBaitInstance.Explode();
        
        TransformIndicator.Instance.DisplayBaitIcon();

    }

    void DoBaitSpawn()
    {
        currentBaitInstance = Instantiate(BaitPrefab, transform.position, Quaternion.identity);
        TransformIndicator.Instance.DisplayExplodeIcon();
    }

    void OnDisable()
    {
        // pop the bait
    }
}
