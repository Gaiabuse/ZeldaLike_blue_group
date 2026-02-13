using UnityEngine;

public class DreamShoot : MonoBehaviour
{
    [SerializeField]
    Projectile attack;

    [SerializeField]
    PlayerController controller;

    [SerializeField]
    float ProjectileSpeed;

    [SerializeField]
    Transform SpawnPoint;

    void OnAttack()
    {
        print("meow");
        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = controller.currentDirection * ProjectileSpeed;
    }
}
