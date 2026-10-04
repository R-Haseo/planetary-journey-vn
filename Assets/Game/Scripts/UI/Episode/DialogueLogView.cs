using System.Text;
using TMPro;
using UnityEngine;

public class DialogueLogView : MonoBehaviour
{
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TMP_Text logText;

    private readonly StringBuilder logBuilder = new();

    public bool IsOpen => logPanel.activeSelf;

    private void Awake()
    {
        logPanel.SetActive(false);
    }

    public void AddDialogue(string speaker, string text)
    {
        if (logBuilder.Length > 0)
        {
            logBuilder.AppendLine();
        }

        logBuilder.AppendLine(speaker);
        logBuilder.AppendLine(text);

        UpdateText();
    }

    public void AddDescription(string text)
    {
        if (logBuilder.Length > 0)
        {
            logBuilder.AppendLine();
        }

        logBuilder.AppendLine(text);

        UpdateText();
    }

    public void Toggle()
    {
        logPanel.SetActive(!logPanel.activeSelf);
    }

    private void UpdateText()
    {
        logText.text = logBuilder.ToString();
    }
}
