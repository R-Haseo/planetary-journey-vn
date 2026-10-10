using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioControlView : MonoBehaviour
{
    [SerializeField] private Button logButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button titleButton;
    [SerializeField] private UIButtonSePlayer buttonSePlayer;

    public void Initialize(
        Action onLogClicked,
        Action onAutoClicked,
        Action onSkipClicked,
        Action onTitleClicked)
    {
        logButton.onClick.AddListener(() =>
        {
            buttonSePlayer.Play();
            onLogClicked?.Invoke();
        });

        autoButton.onClick.AddListener(() =>
        {
            buttonSePlayer.Play();
            onAutoClicked?.Invoke();
        });

        skipButton.onClick.AddListener(() =>
        {
            buttonSePlayer.Play();
            onSkipClicked?.Invoke();
        });

        titleButton.onClick.AddListener(() =>
        {
            StartCoroutine(ReturnToTitleAfterSe(onTitleClicked));
        });
    }

    private IEnumerator ReturnToTitleAfterSe(Action onTitleClicked)
    {
        yield return buttonSePlayer.PlayAndWait();
        onTitleClicked?.Invoke();
    }
}
