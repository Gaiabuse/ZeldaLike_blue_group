using UnityEngine;
using System;

public static class MathHelpers
{
    public static float expDecay(this float From, float To, float decay, float dt)
        => To + (From - To) * MathF.Exp(-decay * dt);
}
