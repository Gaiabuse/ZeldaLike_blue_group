using UnityEngine;

public class SpawnPointSetter : MonoBehaviour
{
    [SerializeField]
    RespawnZone respawnZone;

    private void OnClick()
    {
        SetSpawnPoint();
    }

    private void SetSpawnPoint()
    {
        var respawnPos = respawnZone.GetRespawnPos();

        PlayerPrefs.SetFloat("PlayerSpawnX", respawnPos.x);
        PlayerPrefs.SetFloat("PlayerSpawnY", respawnPos.y);
        PlayerPrefs.SetFloat("PlayerSpawnZ", respawnPos.z);
    }
}
