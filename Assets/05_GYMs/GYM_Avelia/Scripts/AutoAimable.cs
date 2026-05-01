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
    
    public static AutoAimable? GetNearestTargetVisible(Vector3 origin, float radius, LayerMask groundLayer, LayerMask obstacleLayer)
    {
        return GetTargetAround(origin, radius)
            .FirstOrDefault(target => IsPathClear(origin, target.transform.position, groundLayer, obstacleLayer));
    }
#nullable disable

    public static IEnumerable<AutoAimable> GetTargetAround(Vector3 point, float radius)
        => Physics.OverlapSphere(point, radius)
            .Select(a => a.GetComponent<AutoAimable>())
            .Where(a => !(a == null))
            .OrderBy(a => Vector3.Distance(point, a.transform.position) * a.weight);
    private static bool IsPathClear(Vector3 origin, Vector3 targetPos, LayerMask groundLayer, LayerMask obstacleLayer)
    {
        Vector3 eyeLevelOffset = Vector3.up * 1f; 
        if (Physics.Linecast(origin + eyeLevelOffset, targetPos + eyeLevelOffset, obstacleLayer))
        {
            return false; 
        }
        
        float distance = Vector3.Distance(origin, targetPos);
        Vector3 direction = (targetPos - origin).normalized;
    
        for (float i = 0; i < distance; i += 0.5f)
        {
            Vector3 checkPoint = origin + (direction * i);
            
            Vector3 rayStart = checkPoint + (Vector3.up * 2f);
            bool hitGround = Physics.Raycast(rayStart, Vector3.down, 5f, groundLayer);
            
            Debug.DrawRay(rayStart, Vector3.down * 5f, hitGround ? Color.green : Color.red);

            if (!hitGround)
            {
                return false; 
            }
        }

        return true;
    }


}
