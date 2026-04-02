
using System;
using UnityEngine;

public class ArenaEnter : MonoBehaviour
{
    public Action StartArena;

    private bool ArenaIsStarted = false;
    private void OnTriggerEnter(Collider other)
    {
        if(ArenaIsStarted)return;
        if (other.CompareTag("Player"))
        {
            StartArena?.Invoke();
            ArenaIsStarted = true;
        }
    }
}
