using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public int health = 35;
    public int strength;
    public int speed = 5;
    public int speedRotate =7 ;

}
