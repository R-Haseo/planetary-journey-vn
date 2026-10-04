using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private Image leftImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Image rightImage;

    private Vector2 leftBasePosition;
    private Vector2 centerBasePosition;
    private Vector2 rightBasePosition;

    private void Awake()
    {
        leftBasePosition = leftImage.rectTransform.anchoredPosition;
        centerBasePosition = centerImage.rectTransform.anchoredPosition;
        rightBasePosition = rightImage.rectTransform.anchoredPosition;

        InitializeImage(leftImage);
        InitializeImage(centerImage);
        InitializeImage(rightImage);
    }

    public void Show(
        Sprite sprite,
        CharacterPosition position,
        float width,
        float height,
        float offsetX,
        float offsetY)
    {
        var image = GetImage(position);

        var rectTransform = image.rectTransform;
        rectTransform.sizeDelta = new Vector2(width, height);

        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.anchoredPosition = GetBasePosition(position) + new Vector2(offsetX, offsetY);

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

    private Vector2 GetBasePosition(CharacterPosition position)
    {
        return position switch
        {
            CharacterPosition.Left => leftBasePosition,
            CharacterPosition.Center => centerBasePosition,
            CharacterPosition.Right => rightBasePosition,
            _ => centerBasePosition
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
