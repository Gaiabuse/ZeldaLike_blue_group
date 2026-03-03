using UnityEngine;
using System;

public class DreamBait : MonoBehaviour
{
    [SerializeField]
    private GameObject Bait;

    private GameObject currentBaitInstance;

    void Start() { }

    void Update() { }

    void OnBaitInput()
    {
        if (currentBaitInstance == null)
        {

        }
    }

    void DoBaitExplosion() => throw new NotImplementedException($"[TODO] {nameof(DoBaitExplosion)}");
    void DoBaitSpawn() => throw new NotImplementedException($"[TODO] {nameof(DoBaitSpawn)}");
}
