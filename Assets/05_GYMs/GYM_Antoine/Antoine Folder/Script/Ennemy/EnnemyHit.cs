using UnityEngine;

public class EnnemyHit : MonoBehaviour
{
    public int damage = 3;
    [SerializeField] bool canHit;

    Collider col;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && canHit)
        {
            PlayerHP playerHp = collision.GetComponent<PlayerHP>();

            if (playerHp != null) playerHp.TakeDamage(damage);
            else Debug.Log("No Hp Asign to Player");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player") && canHit)
        {
            PlayerHP playerHp = collision.transform.GetComponent<PlayerHP>();

            if (playerHp != null) playerHp.TakeDamage(damage);
            else Debug.Log("No Hp Asign to Player");
        }
    }

    public void ToggleHitBox(bool toogle)
    {
        canHit = toogle;
        col.enabled = toogle;
    }
}
