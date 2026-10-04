using UnityEngine;
using UnityEngine.UI;

public class BackgroundView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;

    public void Show(Sprite sprite)
    {
        backgroundImage.sprite = sprite;
    }
}
