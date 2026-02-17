using UnityEngine;
using System.Linq;

public class AutoAimable : MonoBehaviour
{
    public float weight;

#nullable enable
    public static AutoAimable? GetNearestTargetAround(Vector3 point, float radius)
    {
        var overlaps = Physics.OverlapSphere(point, radius);

        return overlaps.Select(a => a.GetComponent<AutoAimable>())
            .Where(a => !(a == null))
            .OrderBy(a => Vector3.Distance(point, a.transform.position) * a.weight)
            .FirstOrDefault();
    }
#nullable disable
}
