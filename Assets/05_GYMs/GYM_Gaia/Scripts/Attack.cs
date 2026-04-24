using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] GameObject HitSpark;
    [SerializeField] GameObject BlockHitSpark;
    [SerializeField] float distance;

    public enum TypeOfAttack
    {
        Basic,
        Nightmare,
        Dream
    }

    public float manaUsed { private set; get; }
    public float damage { private set; get; }
    [SerializeField] float stun;
    public TypeOfAttack type { private set; get; }

    public Action<bool> Finished;
    private bool touchedEnemy;

    private float knockbackStrength;
    public void SetAttack(AttackData data, TypeOfAttack type)
    {
        this.type = type;
        this.damage = data.damage;
        manaUsed = data.mana;
        knockbackStrength = data.knockBackStrength;
    }
    public void SetAttack(float pDamage, AttackData data, TypeOfAttack type)
    {
        this.type = type;
        this.damage = pDamage;
        manaUsed = data.mana;
    }

    private void Start()
    {
        StartAttack();
    }

    private void StartAttack()
    {
        this.damage = damage;
    }

    public void FinishAttack()
    {

        Finished?.Invoke(touchedEnemy);
        Destroy(gameObject);
    }

    public void TryDoDamage(Collider collider)
    {
        var ennemyScript = collider.transform.GetComponent<IEnemyDamageable>();
        if (ennemyScript == null)
        {
            ennemyScript.TakeDamage((int)damage, stun);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        TryDoDamage(collision);

        if (collision.transform.CompareTag("Ennemy"))
        {
            SheepEnnemy isSheep = collision.GetComponent<SheepEnnemy>();

            KnockBackFeedback knockBackFeedback = collision.GetComponent<KnockBackFeedback>();
            touchedEnemy = true;

            if (isSheep != null)
            {
                if (isSheep.shellHere)
                {
                    if (BlockHitSpark != null) SpawnSpark(BlockHitSpark);
                }
                else if (HitSpark != null) SpawnSpark(HitSpark);
            }
            else if (HitSpark != null) SpawnSpark(HitSpark);

            if (knockBackFeedback != null)
            {
                knockBackFeedback.PlayKnockBack(transform.parent != null ? transform.parent : transform,
                    knockbackStrength);
            }
        }

        if (collision.transform.CompareTag("Garbage"))
        {
            GarbageBehaviors dust = collision.transform.GetComponent<GarbageBehaviors>();
            if (dust != null)
            {
                dust.Clean();
            }
            touchedEnemy = true;
        }
    }

    void SpawnSpark(GameObject spark)
    {
        Transform hitspark = Instantiate(spark).transform;
        hitspark.parent = transform;
        hitspark.localPosition = new Vector3(0, 0, distance);
        hitspark.parent = null;

        Destroy(hitspark.gameObject, 1.5f);
    }
}

public interface IDamageable
{
    void TakeDamage(int damage, float stun = 0f);
}

public interface IEnemyDamageable : IDamageable { }
public interface IPlayerDamageable : IDamageable { }
