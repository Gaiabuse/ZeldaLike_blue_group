using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Physics;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    [SerializeField] private NavMeshSurface  nm;
    
    public static NavMeshManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Rebake(Bounds areaToRebake)
    {
        
    }
}
