using System;
using System.Collections;
using UnityEngine;

public class BomberAttack : MonoBehaviour
{
    [SerializeField] private GameObject bomb;
    [SerializeField] private AnimationCurve bombCurve;
    [SerializeField] private float launchSpeed;
    [SerializeField] private float launchHeight;
    [SerializeField] private Transform player;
    [SerializeField] private bool isLaunching;

    private void OnValidate()
    {
        if (isLaunching)
        {
            Launch();
        }
    }

    public void Launch()
    {
        GameObject newBomb = Instantiate(bomb, transform.position, transform.rotation);
        StartCoroutine(Fire(newBomb, transform.position, player.position));
        isLaunching = false;
    }

    public IEnumerator Fire(GameObject bomb, Vector3 startPos, Vector3 targetPos)
    {
        float timePassed = 0f;
        
        Vector3 destination = targetPos;
        destination.y -= 1;

        while (timePassed < launchSpeed)
        {
            float linearT = timePassed / launchSpeed;
            float heightT = bombCurve.Evaluate(linearT);
            float heightOffset = heightT * launchHeight;
            
            Vector3 currentPos = Vector3.Lerp(startPos, destination, linearT);
            currentPos.y += heightOffset;
        
            bomb.transform.position = currentPos;
        
            timePassed += Time.deltaTime;
            yield return null;
        }
        bomb.transform.position = destination;
    }
}
