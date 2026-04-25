using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxAngle = 90;

    [SerializeField] private int numberInteractionNeed = 1;

    private int currentInteraction = 0;
    private bool doorOpen = false;

    private Vector3 startRotation;
    private TweenerCore<Quaternion, Vector3, QuaternionOptions> doorTween;
    private void Start()
    {
        currentInteraction = 0;
        startRotation = transform.rotation.eulerAngles;
    }

    private void OpenDoor()
    {
        if(doorOpen || doorTween != null)return;
        doorOpen = true;
        if(doorTween != null)doorTween.Kill();
        doorTween = transform.DORotate(new Vector3(0, maxAngle, 0), rotationSpeed).OnComplete(()=>
        {
            doorTween = null;
        });
    }

    private void CloseDoor()
    {
        if(!doorOpen)return;
        doorOpen = false;
        if(doorTween != null)doorTween.Kill();
        doorTween = transform.DORotate(startRotation, rotationSpeed).OnComplete(()=>
        {
            doorTween = null;
        });
    }

    
    private void CheckInteraction()
    {
        if (currentInteraction >= numberInteractionNeed)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    public void AddInteraction()
    {
        currentInteraction++;
        CheckInteraction();
    }

    public void RemoveInteraction()
    {
        currentInteraction--;
        CheckInteraction();
    }
}
