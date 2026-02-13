using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    public Vector3 speed;

    void Update()
    {
        transform.transform.position += speed * Time.deltaTime;
    }
}
