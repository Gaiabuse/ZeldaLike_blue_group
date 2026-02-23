using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public int health = 35;
    public int strength;

    [Header("Speed Patrol")]

    public float speed = 3.5f;
    public int acceleration = 8;
    public int speedRotate = 120;

    [Header("Speed Chase")]

    public float chasespeed = 5f;
    public int chaseacceleration = 9;
    public int chasespeedRotate = 250;
}
