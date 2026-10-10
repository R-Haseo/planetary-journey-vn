using System;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioControlView : MonoBehaviour
{
    [SerializeField] private Button logButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button titleButton;

    public void Initialize(
        Action onLogClicked,
        Action onAutoClicked,
        Action onSkipClicked,
        Action onTitleClicked)
    {
        logButton.onClick.AddListener(() => onLogClicked?.Invoke());
        autoButton.onClick.AddListener(() => onAutoClicked?.Invoke());
        skipButton.onClick.AddListener(() => onSkipClicked?.Invoke());
        titleButton.onClick.AddListener(() => onTitleClicked?.Invoke());
    }
}
