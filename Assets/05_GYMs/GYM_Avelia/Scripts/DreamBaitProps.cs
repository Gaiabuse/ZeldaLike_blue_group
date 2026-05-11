using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.VFX;

public class DreamBaitProps : MonoBehaviour, IPlayerDamageable
{

    [SerializeField] private VisualEffect explosionVFX;
    [SerializeField] private GameObject visual;
    [SerializeField] private int damages;
    [SerializeField] private float radius;

    [SerializeField]
    private float SecondActive = 0.7f;

    [SerializeField]
    private int health = 50;

    private bool invicible = false;

    public async Task Explode()
    {
        invicible = true;
        explosionVFX.enabled = true;
        visual.SetActive(false);

        var enemiesAim = AutoAimable.GetTargetAround(transform.position, radius);
        foreach (AutoAimable enemy in enemiesAim)
        {
            EnnemyBase ennemyBase = enemy.GetComponent<EnnemyBase>();
            if (ennemyBase != null)
            {
                ennemyBase.TakeDamage(damages, 0.1f);
            }
        }
        await Task.Delay((int)(SecondActive * 1000));
        
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
