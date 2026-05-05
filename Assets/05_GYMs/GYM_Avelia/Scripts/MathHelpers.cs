using UnityEngine;
using System;

public static class MathHelpers
{
    public static float expDecay(this float From, float To, float decay, float dt)
        => To + (From - To) * MathF.Exp(-decay * dt);

    public static Vector2 DeconstructIn2d(this Vector3 vector)
        => new(vector.x, vector.z);

    public static float GetAngle(this Vector2 vector)
        => Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;

    public static Vector2 ToVec(this float angle)
        => new(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
}
