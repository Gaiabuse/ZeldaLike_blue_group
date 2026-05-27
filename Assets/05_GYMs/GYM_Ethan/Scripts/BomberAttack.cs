using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BomberAttack : MonoBehaviour
{
    [SerializeField] private GameObject bomb;
    [SerializeField] private AnimationCurve bombCurve;
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float launchSpeed;
    [SerializeField] private float launchHeight;
    [SerializeField] private Transform player;
    [SerializeField] private bool isLaunching;
    [SerializeField] private bool isStriking;

    private void OnValidate()
    {
        if (isLaunching)
        {
            StartCoroutine(LaunchProcedure());
        }

        if (isStriking)
        {
            StartCoroutine(StartStrikeZone(3, player, 5f));
        }
    }

    private IEnumerator StartStrikeZone(int nb, Transform target, float radius)
    {
        List<(GameObject, Vector3)> bombs = new List<(GameObject, Vector3)>();
        
        for (int i = 0; i < nb; i++)
        {
            Vector3 targetPos = target.position;
            targetPos += new Vector3(Random.Range(-radius,radius), 0, Random.Range(-radius,radius));
            GameObject newBomb = Instantiate(bomb, transform.position, transform.rotation);
            bombs.Add((newBomb, targetPos));
            newBomb.GetComponent<StarBomb>().ShowPreview(targetPos, player);
        }
        yield return new WaitForSeconds(chargeSpeed);
        foreach ((GameObject, Vector3) bomb in bombs)
        {
            StartCoroutine(Fire(bomb.Item1, transform.position, bomb.Item2));
        }
        isStriking = false;
    }

    private IEnumerator LaunchProcedure()
    {
        Vector3 target = player.position;
        
        GameObject newBomb = Instantiate(bomb, transform.position, bomb.transform.rotation); 
        newBomb.GetComponent<StarBomb>().ShowPreview(player.position, player);
        yield return new WaitForSeconds(chargeSpeed);
        StartCoroutine(Fire(newBomb, transform.position, target));
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
        bomb.GetComponent<StarBomb>().StartCountdown();
    }
}
