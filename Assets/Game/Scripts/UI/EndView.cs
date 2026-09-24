using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button restartButton;

    public void Initialize(Action onRestart)
    {
        messageText.text = "Coming Soon...";

        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(() => onRestart());

        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
    }
}
