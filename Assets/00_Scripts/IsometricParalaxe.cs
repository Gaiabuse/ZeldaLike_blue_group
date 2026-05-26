using System;
using UnityEngine;

public class IsometricParalaxe : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector2 parallaxMove;

    public void Move(Vector2 move)
    {
        parallaxMove = -move;
    }

    private void FixedUpdate()
    {
        transform.localPosition += (Vector3)parallaxMove * (speed * Time.fixedDeltaTime);
    }
}
