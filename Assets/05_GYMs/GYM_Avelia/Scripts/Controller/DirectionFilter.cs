using UnityEngine;
using System;
using System.Linq;

public interface IHasProjectPoints
{
    Vector3 ProjectPoint(Vector2 point);
}

public class DirectionFilter : MonoBehaviour
{
    [SerializeField]
    private IHasProjectPoints player;

    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("The Range of the AutoAim")]
    private float autoAimRadius = 2f;

    [SerializeField]
    [Range(0f, 360f)]
    [Tooltip("the strength of the assist \n 0 => no assist \n 360 => you can only assist")]
    private float strength = 1f;

    [SerializeField]
    [Tooltip("the strength of the snap higher -> snappier, should be less than 5 probably ? ")]
    private uint SnapStrength = 1;

    [SerializeField]
    [Tooltip("The number of Enemy the game will assist 0 = no assist, must be positive")]
    [Range(0, 50)]
    private int maxNumberOfEnemy = 5;

    void Start()
    {
        // usage of Physics.SphereCast is not good too
        Debug.LogWarning($"heavy usage of {nameof(System.Linq)} in {nameof(FilterStickInput)} it could cause some performance problem", this);
    }

    public Vector3 FilterStickInput(Vector2 direction)
    {

        var aimableNearRaw = AutoAimable.GetTargetAround(transform.position, autoAimRadius);

        if (aimableNearRaw.Count() <= 0) return player.ProjectPoint(direction) * -Mathf.Rad2Deg;

        var position = DeconstructIn2d(transform.position);
        var forwardDir = Vector2.right;

        var angleOfDir = Vector2.SignedAngle(forwardDir, direction);


        // [WARNING] not optimal but I need to go fast or I'll never be able to test it
        var aimableNear = aimableNearRaw
            .Take(maxNumberOfEnemy)
            .Select(x => DeconstructIn2d(x.transform.position))
            .Select(x => position - x)
            .Select(x => Vector2.SignedAngle(forwardDir, x))
            .OrderBy(x => angleOfDir - x)
            .First();

        var finalAngle = angleOfDir + AttractTo(angleOfDir, aimableNear);

        finalAngle *= Mathf.Deg2Rad;

        return player.ProjectPoint(new(MathF.Cos(finalAngle), Mathf.Sin(finalAngle)));
    }

    public float FilterStickInputToAngle(Vector2 direction)
    {
        var aimableNearRaw = AutoAimable.GetTargetAround(transform.position, autoAimRadius);

        if (aimableNearRaw.Count() <= 0) return Mathf.Atan2(direction.y, direction.x) * -Mathf.Rad2Deg;

        var position = DeconstructIn2d(transform.position);
        var forwardDir = Vector2.right;

        var angleOfDir = Vector2.SignedAngle(forwardDir, direction);


        // [WARNING] not optimal but I need to go fast or I'll never be able to test it
        var aimableNear = aimableNearRaw
            .Take(maxNumberOfEnemy)
            .Select(x => DeconstructIn2d(x.transform.position))
            .Select(x => position - x)
            .Select(x => Vector2.SignedAngle(forwardDir, x))
            .OrderBy(x => angleOfDir - x)
            .First();

        var finalAngle = angleOfDir + AttractTo(angleOfDir, aimableNear);

        //finalAngle *= Mathf.Deg2Rad;

        return finalAngle;
    }
    // all that should be in a math helper class but tbh I'm just too lazy rn

    private float gaussian(float x)
        => Mathf.Exp(-0.5f * Mathf.Pow(x, SnapStrength * 2f) / Mathf.Pow(strength, SnapStrength * 2f));

    private float AttractTo(float x, float to)
        => Mathf.Sin(x / strength) * WeightTo(x, to) * strength;

    private float WeightTo(float x, float to)
        => gaussian(x - to);

    private Vector2 DeconstructIn2d(Vector3 vector)
        => new(vector.x, vector.z);

}
