using UnityEngine;
using System;

public class DreamBait : MonoBehaviour
{
    [SerializeField]
    private DreamBaitProps Bait;

    private DreamBaitProps currentBaitInstance;

    void Start() { }

    void Update() { }

    void OnBaitInput()
    {
        if (currentBaitInstance == null)
        {
            DoBaitSpawn();
            return;
        }

        DoBaitExplosion();
    }

    void DoBaitExplosion() => currentBaitInstance.Explode();
    void DoBaitSpawn()
    {
        currentBaitInstance = Instantiate(Bait);

    }
}
