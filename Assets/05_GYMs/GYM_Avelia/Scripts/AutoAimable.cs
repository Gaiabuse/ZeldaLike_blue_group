using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AutoAimable : MonoBehaviour
{

    [SerializeField]
    [Tooltip("lower weight (toward 0) means higher priority, 0 means it will always target that object")]
    float weight = 1f;

#nullable enable
    public static AutoAimable? GetNearestTargetAround(Vector3 point, float radius)
        => GetTargetAround(point, radius)
            .FirstOrDefault();
#nullable disable

    public static IEnumerable<AutoAimable> GetTargetAround(Vector3 point, float radius)
        => Physics.OverlapSphere(point, radius)
            .Select(a => a.GetComponent<AutoAimable>())
            .Where(a => !(a == null))
            .OrderBy(a => Vector3.Distance(point, a.transform.position) * a.weight);


}
