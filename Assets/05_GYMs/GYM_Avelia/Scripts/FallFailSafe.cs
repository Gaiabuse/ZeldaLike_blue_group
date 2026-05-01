using System;
using UnityEngine;

public class FallFailSafe : MonoBehaviour
{
    [SerializeField]
    float yLevelToDespawn;

    [SerializeField]
    Vector3 RespawnPoint;

    void Update()
    {
        if (transform.position.y < yLevelToDespawn)
        {
            transform.position = RespawnPoint;
        }
    }
}
