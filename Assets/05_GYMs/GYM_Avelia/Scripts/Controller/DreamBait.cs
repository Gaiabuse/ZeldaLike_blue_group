using UnityEngine;
using System;
using System.Threading.Tasks;

public class DreamBait : MonoBehaviour
{
    [SerializeField]
    private DreamBaitProps BaitPrefab;

    private DreamBaitProps currentBaitInstance;

    void Start() { }

    void Update() { }

    async Task OnSecondPower()
    {
        if (currentBaitInstance == null)
        {
            DoBaitSpawn();
            return;
        }

        await currentBaitInstance.Explode();
        Destroy(currentBaitInstance);

    }

    void DoBaitSpawn()
    {
        currentBaitInstance = Instantiate(BaitPrefab);

    }
}
