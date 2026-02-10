using System.Threading.Tasks;
using UnityEngine;

public class EphemereObject : MonoBehaviour
{
    [SerializeField]
    float lifeTimeSeconds;

    async Task Start()
    {
        await Task.Delay((int)(lifeTimeSeconds * 1000));

        Destroy(gameObject);
    }

}
