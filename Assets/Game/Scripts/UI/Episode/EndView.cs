using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button backToTitleButton;

    public void Initialize(string message, Action onBackToTitle)
    {
        messageText.text = message;

        backToTitleButton.onClick.RemoveAllListeners();
        backToTitleButton.onClick.AddListener(() => onBackToTitle());

        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
    }
}
