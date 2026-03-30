using UnityEngine;

public class EnnemyHit : MonoBehaviour
{
    public int damage = 3;
    bool canHit;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player") && canHit)
        {
            PlayerHP playerHp = collision.GetComponent<PlayerHP>();

            if (playerHp != null) playerHp.TakeDamage(damage);
            else Debug.Log("No Hp Asign to Player");
        }
    }

    public void ToggleHitBox(bool toogle)
    {
        canHit = toogle;
    }
}
