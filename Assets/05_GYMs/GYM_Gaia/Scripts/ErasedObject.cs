using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;

public class ErasedObject : MonoBehaviour
{
    [SerializeField]private GameObject erasedObject;
    [SerializeField]private Image createIcon;
    [SerializeField]private Image createPointsIcon;
    [SerializeField]private List<Sprite> createPointsSprite;
    
    [Range(1,3)] public int creationCost;
    
    private bool _isCreated;
    private MeshRenderer renderer;
    private MaterialPropertyBlock _propertyBlock;
    public bool Erased { get; private set; }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        renderer = erasedObject.GetComponent<MeshRenderer>();
        Erase();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !_isCreated)
        {
            createPointsIcon.sprite = createPointsSprite[creationCost-1];
            createPointsIcon.enabled = true;
            createIcon.enabled = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            createPointsIcon.enabled = false;
            createIcon.enabled = false;
        }
    } 

    public void Erase()
    {
        DOTween.To(() => 0f, x => 
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat("_Dissolve", x);
                renderer.SetPropertyBlock(_propertyBlock);
            }, 1f, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                _isCreated = false;
                createPointsIcon.enabled = false;
                createIcon.enabled = false;
                Erased = true;
                erasedObject.layer = LayerMask.NameToLayer("ErasedObject");
            });
    }

    public void Create()
    {
        DOTween.To(() => 1f, x => 
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat("_Dissolve", x);
                renderer.SetPropertyBlock(_propertyBlock);
            }, 0f, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                _isCreated = true;
                createPointsIcon.enabled = false;
                createIcon.enabled = false;
                Erased = false;
                erasedObject.layer = LayerMask.NameToLayer("Ground");
            });
    }
}
