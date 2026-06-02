using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float waitBeforeTickDamage = 0.07f;
    [SerializeField] bool multihit;

    float timer = 0;

    PlayerHP playerHp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !multihit)
        {
            if (playerHp == null) playerHp = other.GetComponent<PlayerHP>();
            playerHp.TakeDamage(damage, 0);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && multihit)
        {
            if (playerHp == null) playerHp = other.GetComponent<PlayerHP>();
            if (timer <= 0)
            {
                playerHp.TakeDamage(damage, 0);
                timer = waitBeforeTickDamage;
            }
            timer -= Time.deltaTime;
        }
    }
}
