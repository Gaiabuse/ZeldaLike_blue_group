using System.Threading.Tasks;
using UnityEngine;

public class SimpleAttack : MonoBehaviour
{
    [SerializeField]
    GameObject attack;
    [SerializeField]
    float lifeTimeSeconds = 1;

    // cooldown ?

    async Task OnAttack()
    {
        var lAttack = Instantiate(attack);

        await Task.Delay((int)(lifeTimeSeconds * 1000));

        Destroy(lAttack);
    }
}
