using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button backToTitleButton;
    [SerializeField] private UIButtonSePlayer buttonSePlayer;

    public void Initialize(string message, Action onBackToTitle)
    {
        messageText.text = message;

        backToTitleButton.onClick.RemoveAllListeners();
        backToTitleButton.onClick.AddListener(() =>
        {
            StartCoroutine(ReturnToTitleAfterSe(onBackToTitle));
        });

        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
    }

    private IEnumerator ReturnToTitleAfterSe(Action onBackToTitle)
    {
        yield return buttonSePlayer.PlayAndWait();
        onBackToTitle?.Invoke();
    }
}
