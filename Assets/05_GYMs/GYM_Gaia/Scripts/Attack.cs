using System;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] GameObject HitSpark;

    public enum TypeOfAttack
    {
        Basic,
        Nightmare,
        Dream
    }
    private ManaGauge manaGauge;

    public float manaUsed { private set; get; }
    public float damage { private set; get; }
    public TypeOfAttack type { private set; get; }

    public Action<bool> Finished;
    private bool touchedEnemy;

    public void SetAttack(AttackData data, TypeOfAttack type, ManaGauge manaGauge)
    {
        this.type = type;
        this.damage = data.damage;
        manaUsed = data.mana;
        this.manaGauge = manaGauge;
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

            ennemyScript.TakeDamage((int)damage);

            Transform hitspark = Instantiate(HitSpark).transform;
            hitspark.parent = transform;
            hitspark.localPosition = new Vector3(0, 0, 0.5f);
            hitspark.parent = null;

            touchedEnemy = true;
            Destroy(hitspark.gameObject, 1.5f);
        }
    }
}
