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
    [Tooltip("the range that will attract the direction")]
    private float AttractionRadius = 60f;

    [SerializeField]
    [Tooltip("the strength of the snap higher -> snappier, should be less than 5 probably ? ")]
    [Min(1)]
    private uint SnapStrength = 25;

    [SerializeField]
    [Tooltip("The number of Enemy the game will assist 0 = no assist, must be positive")]
    [Range(0, 50)]
    private int maxNumberOfEnemy = 5;

    void Start()
    {
        // usage of Physics.SphereCast is not good too
        Debug.LogWarning(
            $"Custom [Warning]: heavy usage of {nameof(System.Linq)} in {nameof(FilterStickInput)} it could cause some performance problem",
            this
            );
    }

    public Vector3 FilterStickInput(Vector2 direction)
    {
        var aimableNearRaw = AutoAimable.GetTargetAround(transform.position, autoAimRadius);

        var angleOfDir = direction.GetAngle();

        if (aimableNearRaw.Count() <= 0) return player.ProjectPoint(angleOfDir.ToVec());

        var position = transform.position.DeconstructIn2d();

        // [WARNING] not optimal but I need to go fast or I'll never be able to test it
        var aimableNear = aimableNearRaw
            .Take(maxNumberOfEnemy)
            .Select(x => x.transform.position.DeconstructIn2d())
            .Select(x => x - position)
            .OrderBy(x => Vector2.Angle(direction, x))
            .First();

        var angleOfNearestEnemy = aimableNear.GetAngle();
        var finalAngle = angleOfDir - AttractTo(angleOfDir, angleOfNearestEnemy);

        //finalAngle *= Mathf.Deg2Rad;

        return player.ProjectPoint(finalAngle.ToVec());
    }

    public float FilterStickInputToAngle(Vector2 direction)
    {
        var aimableNearRaw = AutoAimable.GetTargetAround(transform.position, autoAimRadius);

        var angleOfDir = direction.GetAngle();

        if (aimableNearRaw.Count() <= 0) return angleOfDir;

        var position = transform.position.DeconstructIn2d();

        // [WARNING] not optimal but I need to go fast or I'll never be able to test it
        var aimableNear = aimableNearRaw
            .Take(maxNumberOfEnemy)
            .Select(x => x.transform.position.DeconstructIn2d())
            .Select(x => x - position)
            .OrderBy(x => Vector2.Angle(direction, x))
            .First();

        var angleOfNearestEnemy = aimableNear.GetAngle();
        var finalAngle = angleOfDir - AttractTo(angleOfDir, angleOfNearestEnemy);

        //finalAngle *= Mathf.Deg2Rad;

        return finalAngle;
    }
    // all that should be in a math helper class but tbh I'm just too lazy rn

    private float AttractTo(float x, float to)
        => AttractFormula(x - to);

    private float AttractFormula(float x)
        => Mathf.Sin(x / AttractionRadius) * gaussian(x) * AttractionRadius;

    private float gaussian(float x)
        => Mathf.Exp(-0.5f * Mathf.Pow(x, SnapStrength * 2f) / Mathf.Pow(AttractionRadius, SnapStrength * 2f));

}
