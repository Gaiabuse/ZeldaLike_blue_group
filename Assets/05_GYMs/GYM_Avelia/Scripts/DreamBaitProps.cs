using UnityEngine;
using System.Threading.Tasks;

public class DreamBaitProps : MonoBehaviour, IPlayerDamageable
{

    [SerializeField]
    private GameObject Explosion;

    [SerializeField]
    private float SecondActive = 0.7f;

    [SerializeField]
    private int health = 50;

    private bool invicible = false;

    public async Task Explode()
    {
        invicible = true;
        Explosion.SetActive(true);
        await Task.Delay((int)(SecondActive * 1000));
        Explosion.SetActive(false);
        Destroy(gameObject);
    }

    public void TakeDamage(int damage, float stun = 0f)
    {
        health -= damage;
        if (health <= 0)
        {
            Explode();
        }
    }

}
