using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float waitBeforeTickDamage = 1f;

    float t = 0;
    private PlayerHP playerHp;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<DreamDash>().IsDashing) return;
            playerHp = other.GetComponent<PlayerHP>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<DreamDash>().IsDashing) return;
            playerHp = other.GetComponent<PlayerHP>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHp = null;
        }
    }
    
    private void FixedUpdate()
    {
        t  += Time.fixedDeltaTime;
        if (t > waitBeforeTickDamage)
        {
            t = 0;
            if (playerHp == null) return;
            playerHp.TakeDamage(damage, 0);
        }
    }
}
