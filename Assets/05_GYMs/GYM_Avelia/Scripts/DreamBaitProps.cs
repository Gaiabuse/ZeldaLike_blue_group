using System;
using UnityEngine;
using System.Threading.Tasks;

public class DreamBaitProps : MonoBehaviour
{
    [SerializeField] private int maxLife = 15;
    [SerializeField] private SimpleAttack explosionAttack;

    private int currentLife;
    [SerializeField]
    private float SecondActive = 0.5f;
    [SerializeField] private bool isDead = false;

    private void Start()
    {
        currentLife = maxLife;
    }

    public async Task Explode()
    {
        explosionAttack.Attack(transform);
        await Task.Delay((int)(SecondActive * 1000));
        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if(isDead)return;
        if (currentLife > 0)
        {
            currentLife -= damage;
        }
        else
        {
            _ = Explode();
            isDead = true;
        }

    }
    
    
}
