using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour
{
    [SerializeField]
    private Image characterImage;

    private void Awake()
    {
        characterImage.enabled = false;
        characterImage.preserveAspect = true;
    }

    public void Show(Sprite sprite)
    {
        characterImage.sprite = sprite;
        characterImage.enabled = true;
    }

    public void Hide()
    {
        characterImage.sprite = null;
        characterImage.enabled = false;
    }

    public void SetPosition(CharacterPosition position)
    {
        var rectTransform = characterImage.rectTransform;
        var anchoredPosition = rectTransform.anchoredPosition;

        anchoredPosition.x = position switch
        {
            CharacterPosition.Left => -500f,
            CharacterPosition.Center => 0f,
            CharacterPosition.Right => 500f,
            _ => 0f
        };

        rectTransform.anchoredPosition = anchoredPosition;
    }
}

public enum CharacterPosition
{
    Left,
    Center,
    Right
}