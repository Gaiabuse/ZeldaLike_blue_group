using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class BrokenObject : MonoBehaviour
{
    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private List<MeshRenderer> brokenParties;
    [SerializeField] private float duration;
    private bool isBroken;
    private BoxCollider collider;

    private void Awake()
    {
        isBroken = false;
        intactObject.SetActive(true);
        brokenObject.SetActive(false);
        collider = GetComponent<BoxCollider>();
        brokenParties = new List<MeshRenderer>();
        foreach (Transform child in brokenObject.transform)
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            brokenParties.Add(mr);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            Debug.Log("trigger");
            Attack attack = other.GetComponent<Attack>();
            if (attack != null)
            {
                (float damage, Attack.TypeOfAttack type) parameters = attack.GetParameters();

                if (parameters.type == Attack.TypeOfAttack.nightmare)
                {
                    Break();
                }
            }
        }
            
    }

    private void Break()
    {
        if(isBroken)return;
        isBroken = true;
        intactObject.SetActive(false);
        brokenObject.SetActive(true);
        collider.enabled = false;
        transparentMaterial.SetTextureOffset("_MainTex", Vector2.zero);
        Material mat = new Material(transparentMaterial);
        foreach (var mesh in brokenParties)
        {
            mesh.material = mat;
        }

        mat.DOFade(0f, duration).SetEase(Ease.InExpo).OnComplete(()=>
        {
            Destroy(this.gameObject);
        });
    }
}
