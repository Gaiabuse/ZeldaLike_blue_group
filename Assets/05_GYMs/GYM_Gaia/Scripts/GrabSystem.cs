using UnityEngine;
using UnityEngine.InputSystem;

public class GrabSystem : MonoBehaviour
{
    [SerializeField] private float rangeForGrab;
    [SerializeField] private float grabStrength;
    [SerializeField] private float rangeForSwallow;
   
    void OnSecondPower(InputValue _input)
    {
        Debug.Log("OnSecondPower");
    }
}
