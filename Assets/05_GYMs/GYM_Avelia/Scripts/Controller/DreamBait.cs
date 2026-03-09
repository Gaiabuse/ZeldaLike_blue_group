using UnityEngine;
using System;
using System.Threading.Tasks;

public class DreamBait : MonoBehaviour
{
    [SerializeField]
    private DreamBaitProps Bait;

    private DreamBaitProps currentBaitInstance;

    void Start() { }

    void Update() { }

    async Task OnBaitInput()
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
        currentBaitInstance = Instantiate(Bait);

    }
}
