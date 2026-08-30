using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private DialogueView dialogueView;

    private readonly List<DialogueLine> lines = new();
    private int currentIndex;

    private void Start()
    {
        lines.Add(new DialogueLine("少女", "……何もないね。"));
        lines.Add(new DialogueLine("AI", "はい。"));
        lines.Add(new DialogueLine("少女", "ずっと？"));
        lines.Add(new DialogueLine("AI", "少なくとも、ここ三日間は。"));

        ShowCurrentLine();
    }

    private void Update()
    {
        var mouseClicked = Mouse.current?.leftButton.wasPressedThisFrame == true;

        var screenTouched = Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true;

        var spacePressed = Keyboard.current?.spaceKey.wasPressedThisFrame == true;

        if (mouseClicked || screenTouched || spacePressed)
        {
            NextLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (currentIndex < lines.Count)
        {
            dialogueView.Show(lines[currentIndex]);
        }
    }

    private void NextLine()
    {
        if (currentIndex >= lines.Count - 1)
        {
            return;
        }

        currentIndex++;
        ShowCurrentLine();
    }
}
