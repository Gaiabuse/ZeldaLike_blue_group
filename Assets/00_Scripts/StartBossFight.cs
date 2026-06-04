using System;
using UnityEngine;

public class StartBossFight : MonoBehaviour
{
    [SerializeField] private DreamCoreManager dreamCoreManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dreamCoreManager.StartBossFight();
            gameObject.SetActive(false);
        }
    }
}
