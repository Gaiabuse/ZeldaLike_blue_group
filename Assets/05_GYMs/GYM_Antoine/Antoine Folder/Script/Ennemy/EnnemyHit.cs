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
        if (!canHit) return;

        var otherDamageablePlayer = collision.GetComponent<IPlayerDamageable>();

        if (otherDamageablePlayer == null) return;

        otherDamageablePlayer.TakeDamage(damage);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canHit) return;

        var otherDamageablePlayer = collision.transform.GetComponent<IPlayerDamageable>();

        if (otherDamageablePlayer == null) return;

        otherDamageablePlayer.TakeDamage(damage);
    }

    public void ToggleHitBox(bool toggle)
    {
        canHit = toggle;
        col.enabled = toggle;
    }
}
