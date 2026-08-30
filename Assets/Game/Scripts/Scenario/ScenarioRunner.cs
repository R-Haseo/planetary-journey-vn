using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private BackgroundView backgroundView;
    [SerializeField] private AudioSource voiceAudioSource;

    [SerializeField] private Sprite spaceExterior;
    [SerializeField] private Sprite spaceshipInterior;
    [SerializeField] private AudioClip girl001;
    [SerializeField] private AudioClip ai002;
    [SerializeField] private AudioClip girl003;
    [SerializeField] private AudioClip ai004;

    private readonly List<DialogueLine> lines = new();
    private int currentIndex;

    private void Start()
    {
        backgroundView.Show(spaceExterior);

        lines.Add(new DialogueLine("少女", "……何もないね。", null, girl001));
        lines.Add(new DialogueLine("AI", "はい。", null, ai002));
        lines.Add(new DialogueLine("少女", "ずっと？", null, girl003));
        lines.Add(new DialogueLine("AI", "少なくとも、ここ三日間は。", null, ai004));

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
        if (currentIndex >= lines.Count)
        {
            return;
        }

        var line = lines[currentIndex];

        dialogueView.Show(line);

        if (line.Background != null)
        {
            backgroundView.Show(line.Background);
        }

        voiceAudioSource.Stop();

        if (line.Voice != null)
        {
            voiceAudioSource.PlayOneShot(line.Voice);
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
