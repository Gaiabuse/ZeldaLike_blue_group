using UnityEngine;
using System.Threading.Tasks;

public class DreamBaitProps : MonoBehaviour
{
    [SerializeField]
    private GameObject Explosion;

    [SerializeField]
    private float SecondActive = 0.7f;

    public async Task Explode()
    {
        Explosion.SetActive(true);
        print("premeow");

        await Task.Delay((int)(SecondActive * 1000));
        print("meow");
        Explosion.SetActive(false);
    }
}
