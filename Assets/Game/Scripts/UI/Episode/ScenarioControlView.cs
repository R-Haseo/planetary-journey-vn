using System;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioControlView : MonoBehaviour
{
    [SerializeField] private Button logButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button skipButton;

    public void Initialize(
        Action onLogClicked,
        Action onAutoClicked,
        Action onSkipClicked)
    {
        logButton.onClick.AddListener(() => onLogClicked?.Invoke());
        autoButton.onClick.AddListener(() => onAutoClicked?.Invoke());
        skipButton.onClick.AddListener(() => onSkipClicked?.Invoke());
    }
}
