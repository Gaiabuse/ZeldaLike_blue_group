using System;
using UnityEngine;
using UnityEngine.Events;

public class RespawnZone : MonoBehaviour
{
   
     [SerializeField]private UnityEvent onPlayerRespawn;
     [SerializeField]private Vector3 playerRespawnPosition;
     private void Start()
     {
         OnPLayerRespawn();
     }

     private void OnEnable()
     {
         PlayerController.OnRespawn += OnPLayerRespawn;
     }

     private void OnDisable()
     {
         PlayerController.OnRespawn += OnPLayerRespawn;
     }

     private void OnTriggerEnter(Collider other)
     {
         if (other.CompareTag("Player"))
         {
             PlayerPrefs.SetFloat("PlayerSpawnX", playerRespawnPosition.x);
             PlayerPrefs.SetFloat("PlayerSpawnY", playerRespawnPosition.y);
             PlayerPrefs.SetFloat("PlayerSpawnZ", playerRespawnPosition.z);
         }
     }

     void OnPLayerRespawn()
    {
        onPlayerRespawn?.Invoke();
    }
}
