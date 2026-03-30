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
    private ManaGauge manaGauge;

    public float manaUsed { private set; get; }
    public float damage{private set; get;}
    public TypeOfAttack type{private set; get;}

    public Action<bool> Finished;
    private bool touchedEnemy;

    private float knockbackStrength;
    public void SetAttack(AttackData data, TypeOfAttack type, ManaGauge manaGauge)
    {
        this.type = type;
        this.damage = data.damage;
        manaUsed = data.mana;
        this.manaGauge = manaGauge;
        knockbackStrength = data.knockBackStrength;
    }
    public void SetAttack(float pDamage, AttackData data, TypeOfAttack type, ManaGauge manaGauge)
    {
        this.type = type;
        this.damage = pDamage;
        manaUsed = data.mana;
        this.manaGauge = manaGauge;
    }

    private void Start()
    {
        StartAttack();
    }
    
    private void StartAttack()
    {
        if (type is not TypeOfAttack.Basic)
        {
            manaGauge.AddMana(-manaUsed);
        }
        this.damage = damage;
    }
    
    public void FinishAttack()
    {
        if (touchedEnemy)
        {
            if (type == TypeOfAttack.Basic)
            {
                manaGauge.AddMana(manaUsed);
            }
        }
        Finished?.Invoke(touchedEnemy);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.CompareTag("Ennemy"))
        {
            Ennemy ennemyScript = collision.transform.GetComponent<Ennemy>();
            if (ennemyScript == null)
            {
                collision.transform.GetComponent<EnnemyBase>().TakeDamage((int)damage);

            }
            else
            {
                ennemyScript.TakeDamage((int)damage);
            }

            SheepEnnemy isSheep = collision.GetComponent<SheepEnnemy>();

            KnockBackFeedback knockBackFeedback = collision.GetComponent<KnockBackFeedback>();
            touchedEnemy = true;
            if (isSheep != null)
            {
                if (isSheep.shellHere)
                {
                    if (BlockHitSpark != null) SpawnSpark(BlockHitSpark);
                }
                else
                {
                    if (HitSpark != null) SpawnSpark(HitSpark);
                }
            }
            else
            {
                if (HitSpark != null) SpawnSpark(HitSpark);
            }

            if (knockBackFeedback != null)
            {
                Debug.Log(transform.parent.name);
                knockBackFeedback.PlayKnockBack(transform.parent, knockbackStrength);
            }
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
