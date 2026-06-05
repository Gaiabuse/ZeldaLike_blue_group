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
        await Awaitable.NextFrameAsync();
    
        // Check 'this' or 'gameObject' directly before accessing any properties
        if (this == null) return; 

        // Safe to proceed now
        var scaleFactor = (attack.damage - MinAttack) / (MaxAttack - MinAttack);
        print(scaleFactor);
        transform.localScale = Vector3.Lerp(MinScale, MaxScale, scaleFactor);
    }

    public void SetMinMax(float min, float max)
    {
        MinAttack = min;
        MaxAttack = max;
    }

}
