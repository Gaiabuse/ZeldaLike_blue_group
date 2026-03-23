using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    public Attack attackPrefab;
    public float damage;
    public float mana;
    public float knockBackStrength =15f;
}
