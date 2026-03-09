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

    public float damage{private set; get;}
    public TypeOfAttack type{private set; get;}

    public Action Finished;

    public void SetAttack(float damage, TypeOfAttack type)
    {
        this.type = type;
        this.damage = damage;
    }
    
    public void FinishAttack()
    {
        Finished?.Invoke();
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

            Destroy(hitspark.gameObject, 1.5f);
        }
    }
}
