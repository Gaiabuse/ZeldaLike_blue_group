using UnityEngine;

public class SpawnAllSpawnPointSetter : MonoBehaviour
{
    [SerializeField]
    SpawnPointSetter setters;

    void Start()
    {
        var respawnPoint = FindObjectsByType<RespawnZone>(FindObjectsSortMode.None);

        foreach (var zone in respawnPoint)
        {
            var button = Instantiate(setters, transform);
        }
    }
}
