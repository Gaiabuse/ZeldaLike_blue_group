using UnityEngine;

public class DirectionFilter : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f, 10f)]
    [Tooltip("The Range of the AutoAim")]
    private float AutoAimRadius = 2f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("the strength of the assist \n 0 => no assist \n 1 => you can only assist")]
    private float strength = 1f;

    [SerializeField]
    [Tooltip("the strength of the snap higher -> snappier")]
    private uint SnapStrength = 1;

    [SerializeField]
    [Tooltip("The number of Enemy the game will assist 0 = no assist")]
    private uint maxNumberOfEnemy = 5;


    public Vector3 FilterStickInput(Vector2 direction)
    {

        throw new System.NotImplementedException();
    }


    private float gaussian(float x)
        => Mathf.Exp(-0.5f * Mathf.Pow(x, SnapStrength * 2f) / strength);

    private float AttractTo(float x, float to)
        => Mathf.Sin(x) - gaussian(x - to);
}
