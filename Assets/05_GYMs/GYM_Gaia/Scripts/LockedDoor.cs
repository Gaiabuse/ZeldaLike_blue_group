using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private ActionButton button;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxAngle = 90;

    [SerializeField] private Vector3 direction;
    private void OnEnable()
    {
        button.OnInteraction += MoveDoor;
    }

    private void OnDisable()
    {
        button.OnInteraction -= MoveDoor;
    }

    private void MoveDoor()
    {
        transform.DORotate(new Vector3(0, maxAngle, 0), rotationSpeed);
    }
}
