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
    [SerializeField] private Transform player;
    
    [SerializeField] private float chargeSpeed;
    [SerializeField] private float launchSpeed;
    [SerializeField] private float launchHeight;
    [SerializeField] private float launcherSafeRadius = 2.23f;
    
    [Header("Launch")]
    [SerializeField] private bool isLaunching;
    
    [Header("Launch Around")]
    [SerializeField] private bool isStriking;
    [SerializeField] private int strikeNb = 3;
    [SerializeField] private float strikeRadius = 2;
    
    [Header("Circle Launch")]
    [SerializeField] private bool isCircleLaunch;
    [SerializeField] private int nbCircleLaunched = 5;
    [SerializeField] private float circleRadius = 4;
    [SerializeField] private float circleLaunchSpeed = 0.5f;
    
    [Header("Random Launch")]
    [SerializeField] private bool isRandomLaunch;
    [SerializeField] private Vector2 nbRandomLaunched = new Vector2(5, 10);
    [SerializeField] private float randomRadius = 4;
    [SerializeField] private Vector2 randomLaunchSpeed = new Vector2(0.1f, 1);

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            isLaunching = false;
            isStriking = false;
            isCircleLaunch = false;
            isRandomLaunch = false;
            return;
        }
        
        if (isLaunching)
        {
            StartCoroutine(LaunchProcedure());
        }

        if (isStriking)
        {
            StartCoroutine(StartStrikeZone(strikeNb, player, strikeRadius));
        }
        
        if (isCircleLaunch)
        {
            StartCoroutine(StartCircleLaunch(nbCircleLaunched, circleRadius, circleLaunchSpeed, player.position.y));
        }
        
        if (isRandomLaunch)
        {
            StartCoroutine(StartRandomLaunch(nbRandomLaunched, randomRadius, randomLaunchSpeed, player.position.y));
        }
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

    private IEnumerator StartStrikeZone(int nb, Transform target, float radius)
    {
        List<(GameObject, Vector3)> bombs = new List<(GameObject, Vector3)>();
        
        Vector3 launcherPos = transform.position; 

        for (int i = 0; i < nb; i++)
        {
            Vector3 targetPos;
            int safetyCounter = 0; 

            do
            {
                targetPos = target.position;
                targetPos += new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
                safetyCounter++;
            } 

            while (Vector3.Distance(new Vector3(targetPos.x, launcherPos.y, targetPos.z), launcherPos) < launcherSafeRadius && safetyCounter < 10);

            GameObject newBomb = Instantiate(bomb, transform.position, bomb.transform.rotation);
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
    
    private IEnumerator StartCircleLaunch(int nb, float radius, float lTime, float yTarget)
    {
        List<(GameObject obj, Vector3 target)> bombs = new List<(GameObject, Vector3)>();
    
        for (int i = 0; i < nb; i++)
        {
            float angle = (360f / nb) * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 targetPos = transform.position + (rotation * Vector3.forward * radius);
            targetPos.y = yTarget;
            
            GameObject newBomb = Instantiate(bomb, transform.position, bomb.transform.rotation);
            bombs.Add((newBomb, targetPos));
        }
        
        foreach (var bombInstance in bombs)
        {
            bombInstance.Item1.GetComponent<StarBomb>().ShowPreview(bombInstance.Item2, player);
            
            yield return new WaitForSeconds(lTime);
            
            if (bombInstance.obj != null)
            {
                StartCoroutine(Fire(bombInstance.obj, transform.position, bombInstance.target));
            }
        }
    
        isCircleLaunch = false;
    }
    
    private IEnumerator StartRandomLaunch(Vector2 rndNb, float radius, Vector2 rndLTime, float yTarget)
    {
        int nb = (int)Random.Range(rndNb.x, rndNb.y);
        List<(GameObject obj, Vector3 target)> bombs = new List<(GameObject, Vector3)>();
    
        Vector3 launcherPos = transform.position; 
        
        for (int i = 0; i < nb; i++)
        {
            Vector3 targetPos;
            int safetyCounter = 0; 

            do
            {
                targetPos = transform.position;
                targetPos += new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
                targetPos.y = yTarget;
                safetyCounter++;
            } 

            while (Vector3.Distance(new Vector3(targetPos.x, launcherPos.y, targetPos.z), launcherPos) < launcherSafeRadius && safetyCounter < 10);

            GameObject newBomb = Instantiate(bomb, transform.position, bomb.transform.rotation);
            bombs.Add((newBomb, targetPos));
        }
        
        foreach (var bombInstance in bombs)
        {
            float lTime = Random.Range(rndLTime.x, rndLTime.y);
            bombInstance.Item1.GetComponent<StarBomb>().ShowPreview(bombInstance.Item2, player);
            Debug.Log(lTime);
            yield return new WaitForSeconds(lTime);
            
            if (bombInstance.obj != null)
            {
                StartCoroutine(Fire(bombInstance.obj, transform.position, bombInstance.target));
            }
        }
    
        isRandomLaunch = false;
    }

    public IEnumerator Fire(GameObject bomb, Vector3 startPos, Vector3 targetPos)
    {
        float timePassed = 0f;
    
        Vector3 destination = targetPos;
        destination.y -= 1;
        
        Vector3 randomTumbleAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        float tumbleSpeed = Random.Range(180f, 360f); 
        
        Quaternion originalRotation = bomb.transform.rotation;

        while (timePassed < launchSpeed)
        {
            float linearT = timePassed / launchSpeed;
            float heightT = bombCurve.Evaluate(linearT);
            float heightOffset = heightT * launchHeight;
            
            Vector3 currentPos = Vector3.Lerp(startPos, destination, linearT);
            currentPos.y += heightOffset;
            bomb.transform.position = currentPos;
            
            if (linearT < 0.8f)
            {
                bomb.transform.Rotate(randomTumbleAxis, tumbleSpeed * Time.deltaTime, Space.Self);
            }
            else
            {
                float settleT = (linearT - 0.8f) / 0.2f;
                bomb.transform.rotation = Quaternion.Slerp(bomb.transform.rotation, originalRotation, settleT);
            }
    
            timePassed += Time.deltaTime;
            yield return null;
        }
        
        bomb.transform.position = destination;
        bomb.transform.rotation = originalRotation; 
        bomb.GetComponent<StarBomb>().StartCountdown();
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Flatten the gizmo matrix on the Y axis (Scale Y is set to 0.01)
        Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, new Vector3(1, 0.01f, 1));

        // 1. Safe Radius (Red) - Note: position is Vector3.zero because the matrix handles the origin
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, launcherSafeRadius);

        // 2. Circle Launch Radius (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, circleRadius);

        // 3. Random Launch Radius (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, randomRadius);
        
        // Restore matrix for the player zone calculation
        Gizmos.matrix = oldMatrix;

        // 4. Player Strike Radius (Green)
        if (player != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(player.position, Quaternion.identity, new Vector3(1, 0.01f, 1));
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Vector3.zero, strikeRadius);
            
            Gizmos.matrix = oldMatrix; // Always restore your matrix at the end
        }
    }
}
