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
    private bool isPlayerInside;
    public bool Erased { get; private set; }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        createIcon = TransformIndicator.Instance.createIconImg;
        createPointsIcon = TransformIndicator.Instance.createPointsIconeateIconImg;
        renderer = erasedObject.GetComponent<MeshRenderer>();
        Erase();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            UpdateUIVisibility();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            UpdateUIVisibility();
        }
    }
    
    private void UpdateUIVisibility()
    {
        bool shouldShow = isPlayerInside && !_isCreated;
        
        if(createIcon)createIcon.enabled = shouldShow;
        if(createPointsIcon)createPointsIcon.enabled = shouldShow;

        if (shouldShow)
        {
            Debug.Log(creationCost - 1);
            createPointsIcon.sprite = createPointsSprite[creationCost - 1];
            TransformIndicator.Instance.StartBlink(creationCost);
        }
        else
        {
            TransformIndicator.Instance.StopBlink(creationCost);
        }
    }

    public void Erase()
    {
        erasedObject.layer = LayerMask.NameToLayer("ErasedObject");
        TransformIndicator.Instance.StopBlink(creationCost);
        _isCreated = false;
        Erased = true;
        DOTween.To(() => 0f, x => 
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat("_Dissolve", x);
                renderer.SetPropertyBlock(_propertyBlock);
            }, 1f, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                UpdateUIVisibility();
            });
    }

    public void Create()
    {
        _isCreated = true;
        Erased = false;
        DOTween.To(() => 1f, x => 
            {
                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat("_Dissolve", x);
                renderer.SetPropertyBlock(_propertyBlock);
            }, 0f, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => 
            {
                erasedObject.layer = LayerMask.NameToLayer("Ground");
                UpdateUIVisibility();
            });
    }
}
