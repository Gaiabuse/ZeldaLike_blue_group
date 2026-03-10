using UnityEngine;

public class ScalingAttack : MonoBehaviour
{
    [SerializeField]
    Attack attack;

    [SerializeField]
    Vector3 MinScale, MaxScale;

    public float MinAttack, MaxAttack;

    async void Start()
    {
        print("meow");
        await Awaitable.NextFrameAsync();

        var scaleFactor = (attack.damage - MinAttack) / (MaxAttack - MinAttack);
        print(scaleFactor);
        transform.localScale = Vector3.Lerp(MinScale, MaxScale, scaleFactor);
    }

    public void SetMinMax(float min, float max)
    {
        print("meow 2");
        MinAttack = min;
        MaxAttack = max;
    }

}
