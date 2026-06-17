using UnityEngine;
using UnityEngine.EventSystems;

public class EnableOnUiSelect : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject textObject;

    public void OnPointerEnter(PointerEventData eventData)
    { 
        if (SteamAchievements.Instance == null || !SteamAchievements.Instance.achLocked) textObject.SetActive(true);
    } 
        
    public void OnPointerExit(PointerEventData eventData) => textObject.SetActive(false);
    public void OnSelect(BaseEventData eventData)
    { 
        if (SteamAchievements.Instance == null || !SteamAchievements.Instance.achLocked) textObject.SetActive(true);
    }
    public void OnDeselect(BaseEventData eventData) => textObject.SetActive(false);
}
