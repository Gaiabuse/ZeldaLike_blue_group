using UnityEngine;
using UnityEngine.InputSystem;

public class DreamShoot : MonoBehaviour
{
    [SerializeField]
    Projectile attack;

    [SerializeField]
    PlayerController controller;

    [SerializeField]
    GameObject aimCone;

    [SerializeField]
    float ProjectileSpeed, autoAimTime = 0.3f;

    [SerializeField]
    Transform SpawnPoint;

    [SerializeField] protected Attack.TypeOfAttack type;
    [SerializeField] protected float damage;

    float lastInputTime;

    void OnAttack(InputValue _input)
    {
        if (_input.isPressed)
        {
            lastInputTime = Time.time;
            controller.CanMove = false;
            // we should try to do something to make things seem more sensitive

            aimCone.SetActive(true);
            return;
        }

        controller.CanMove = true;
        aimCone.SetActive(false);
        var amountOfTimeWaited = Time.time - lastInputTime;

        if (amountOfTimeWaited < autoAimTime)
        {
            CreateAutoTargettingShot();
            return;
        }

        CreateShot();
        return;
    }

    void CreateShot()
    {
        Projectile lAttack = Instantiate<Projectile>(attack);

        lAttack.GetComponent<Attack>().SetAttack(damage, type);
        lAttack.transform.position = SpawnPoint.position;
        lAttack.speed = controller.transform.forward * ProjectileSpeed;
    }

    void CreateAutoTargettingShot()
    {
        // do shit
    }
}
