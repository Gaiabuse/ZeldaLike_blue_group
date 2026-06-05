using UnityEngine;
using TMPro;

public class SpawnPointSetter : MonoBehaviour
{
    [SerializeField]
    TMP_Text text_name;

    private RespawnZone respawnZone;

    public void OnClick()
    {
        SetSpawnPoint();
        RespawnPlayer();
    }

    void SetSpawnPoint()
    {
        var respawnPos = respawnZone.GetRespawnPos();

        PlayerPrefs.SetFloat("PlayerSpawnX", respawnPos.x);
        PlayerPrefs.SetFloat("PlayerSpawnY", respawnPos.y);
        PlayerPrefs.SetFloat("PlayerSpawnZ", respawnPos.z);
    }

    public SpawnPointSetter SetRespawnZone(RespawnZone zone)
    {
        respawnZone = zone;
        text_name.text = zone.name;
        return this;
    }

    private void RespawnPlayer()
    {
        var current_player = FindAnyObjectByType<PlayerController>();
        current_player.TriggerRespawn();
    }
}
