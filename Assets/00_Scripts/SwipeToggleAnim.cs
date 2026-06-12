using UnityEngine;
using UnityEngine.UI;

public class SwipeToggleAnim : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] Sprite[] sprites;
    
    private void Start()
    {
        UpdateImage();
    }

    public void UpdateImage()
    {
        SpriteState state = toggle.spriteState;
        state.selectedSprite = sprites[toggle.isOn ? 1 : 0];
        toggle.spriteState = state;
    }
}
