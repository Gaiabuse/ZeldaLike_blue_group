using System;
using UnityEngine;
using UnityEngine.VFX;

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

    public Action Finished;
    public Action FinishedAttackFull;
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

    public void StartCombo()
    {
        Debug.Log("startCombo");
        Finished?.Invoke();
    }

    public void FinishAttack()
    {
        Debug.Log("finishAttack");
        FinishedAttackFull?.Invoke();
        Destroy(gameObject);
    }

    public void TryDoDamage(Collider collider)
    {
        EnnemyBase ennemyScript = collider.transform.GetComponent<EnnemyBase>();
        if (ennemyScript != null)
        {
            ennemyScript.TakeDamage((int)damage, stun);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        TryDoDamage(collision);

        if (collision.transform.CompareTag("Ennemy"))
        {
            SheepEnnemyTest isSheep = collision.GetComponent<SheepEnnemyTest>();
            KnockBackFeedback knockBackFeedback = collision.GetComponent<KnockBackFeedback>();
            touchedEnemy = true;

            if (isSheep)
            {
                if (isSheep.shellHere)
                {
                    if (BlockHitSpark) SpawnSpark(BlockHitSpark);
                }
                else if (HitSpark) SpawnSpark(HitSpark);
            }
            else if (HitSpark) SpawnSpark(HitSpark);

            if (knockBackFeedback != null)
            {
                knockBackFeedback.PlayKnockBack(transform.parent != null ? transform.parent : transform,
                    knockbackStrength);
            }
        }
        if (collision.transform.CompareTag("DreamCore"))
        {
            DreamCoreManager dreamCore = collision.transform.GetComponent<DreamCoreManager>();
            if (dreamCore != null)
            {
                dreamCore.TakeDamages((int)damage);
            }
            touchedEnemy = true;
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
        if (collision.transform.CompareTag("Glue"))
        {
            GarbageBehaviors dust = collision.transform.GetComponentInParent<GarbageBehaviors>();
            if (dust != null)
            {
                dust.Clean();
                dust.PlayGlueVFX(collision.transform);
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
