using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public int health = 35;
    public int strength;
    public int speed = 5;
    public int acceleration = 8;
    public int chasespeed = 12;

    public int speedRotate =7 ;
    public int chaseacceleration = 8;
    public int chasespeedRotate = 14;
}
