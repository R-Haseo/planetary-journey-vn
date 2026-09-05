using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour
{
    [SerializeField]
    private Image leftImage;

    [SerializeField]
    private Image centerImage;

    [SerializeField]
    private Image rightImage;

    private void Awake()
    {
        InitializeImage(leftImage);
        InitializeImage(centerImage);
        InitializeImage(rightImage);
    }

    public void Show(Sprite sprite, CharacterPosition position)
    {
        var image = GetImage(position);

        image.sprite = sprite;
        image.enabled = true;
    }

    public void Hide(CharacterPosition position)
    {
        var image = GetImage(position);

        image.sprite = null;
        image.enabled = false;
    }

    private Image GetImage(CharacterPosition position)
    {
        return position switch
        {
            CharacterPosition.Left => leftImage,
            CharacterPosition.Center => centerImage,
            CharacterPosition.Right => rightImage,
            _ => centerImage
        };
    }

    private static void InitializeImage(Image image)
    {
        image.sprite = null;
        image.preserveAspect = true;
        image.enabled = false;
    }
}

public enum CharacterPosition
{
    Left,
    Center,
    Right
}
