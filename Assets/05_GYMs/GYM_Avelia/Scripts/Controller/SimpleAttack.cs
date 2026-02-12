using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleAttack : MonoBehaviour
{
    [SerializeField]
    protected Attack attack;
    [SerializeField]
    protected float lifeTimeSeconds = 0.3f;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField] protected float damage;
    [SerializeField]protected PlayerController player;
    // cooldown ?
    private void OnEnable()
    {
        player.Attack += Attack;
    }

    private void OnDisable()
    {
        player.Attack -= Attack;
    }

    private void Attack()
    {
        StartCoroutine(OnAttackEnumerator());
    }
    public virtual IEnumerator OnAttackEnumerator()
    {
        var lAttack = Instantiate(attack,player.transform);
        lAttack.SetAttack(damage, type);

        yield return new WaitForSeconds(lifeTimeSeconds);
        Destroy(lAttack.gameObject);
    }
}
