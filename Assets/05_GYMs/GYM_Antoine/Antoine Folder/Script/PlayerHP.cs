using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int HP = 15;

    public void TakeDamage(int damage)
    {
        if (HP > 0)
        {
            HP -= damage;
            Debug.Log("Outch");
        }
        if (HP <= 0)
        {
            Debug.Log("Dead");
            Destroy(gameObject);
        }
    }
}
