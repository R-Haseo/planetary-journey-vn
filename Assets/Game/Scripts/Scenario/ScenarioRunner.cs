using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private CharacterPlayer characterPlayer;
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private BackgroundView backgroundView;
    [SerializeField] private DialogueLogView dialogueLogView;
    [SerializeField] private VoicePlayer voicePlayer;

    [SerializeField] private TextAsset scenarioJson;
    [SerializeField] private List<BackgroundEntry> backgrounds;

    [SerializeField] private float skipInterval = 0.08f;

    private List<ScenarioCommandDto> commands;
    private int currentIndex;
    private float skipTimer;

    private void Awake()
    {
        var scenarioData = JsonUtility.FromJson<ScenarioDataDto>(scenarioJson.text);

        commands = scenarioData.Commands;

        Debug.Log($"Scenario loaded: {commands?.Count ?? 0} commands");
    }

    private void Start()
    {
        currentIndex = 0;
        ProcessCurrentCommand();
    }

    private void Update()
    {
        var logPressed = Keyboard.current?.lKey.wasPressedThisFrame == true;

        if (logPressed)
        {
            dialogueLogView.Toggle();
            return;
        }

        if (dialogueLogView.IsOpen)
        {
            return;
        }

        var mouseClicked = Mouse.current?.leftButton.wasPressedThisFrame == true;
        var screenTouched = Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true;
        var spacePressed = Keyboard.current?.spaceKey.wasPressedThisFrame == true;

        if (mouseClicked || screenTouched || spacePressed)
        {
            NextLine();
        }

        UpdateSkip();
    }

    private void UpdateSkip()
    {
        var keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        var skipPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;

        if (!skipPressed)
        {
            skipTimer = 0f;
            return;
        }

        skipTimer += Time.unscaledDeltaTime;

        if (skipTimer < skipInterval)
        {
            return;
        }

        skipTimer = 0f;

        NextLine();
    }

    private void NextLine()
    {
        if (currentIndex >= commands.Count)
        {
            return;
        }

        var scenarioCommand = commands[currentIndex];

        if (scenarioCommand.Type != "dialogue" && scenarioCommand.Type != "description")
        {
            return;
        }

        voicePlayer.Stop();

        MoveToNextCommand();
    }

    private void MoveToNextCommand()
    {
        currentIndex++;
        ProcessCurrentCommand();
    }

    private void ProcessCurrentCommand()
    {
        if (currentIndex >= commands.Count)
        {
            Debug.Log("End of scenario");
            return;
        }

        var scenarioCommand = commands[currentIndex];

        switch (scenarioCommand.Type)
        {
            case "character":
                ProcessCharacterCommand(scenarioCommand);
                break;

            case "dialogue":
                ShowDialogue(scenarioCommand);
                break;

            case "background":
                ShowBackground(scenarioCommand.AssetId);
                MoveToNextCommand();
                break;

            case "description":
                ShowDescription(scenarioCommand);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown scenario command type: {scenarioCommand.Type}");

                MoveToNextCommand();
                break;
        }
    }

    private void ProcessCharacterCommand(ScenarioCommandDto command)
    {
        var position = ParseCharacterPosition(command.Position);

        switch (command.Action)
        {
            case "show":
                characterPlayer.Show(
                    command.AssetId,
                    position,
                    command.Width,
                    command.Height,
                    command.OffsetX,
                    command.OffsetY);
                break;

            case "hide":
                characterPlayer.Hide(position);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown character action: {command.Action}");
                break;
        }

        MoveToNextCommand();
    }

    private CharacterPosition ParseCharacterPosition(string position)
    {
        return position switch
        {
            "left" => CharacterPosition.Left,
            "right" => CharacterPosition.Right,
            _ => CharacterPosition.Center
        };
    }

    private void ShowDialogue(ScenarioCommandDto scenarioCommand)
    {
        dialogueView.Show(scenarioCommand.Speaker, scenarioCommand.Text);
        dialogueLogView.AddDialogue(scenarioCommand.Speaker, scenarioCommand.Text);
        voicePlayer.Play(scenarioCommand.Id);
    }

    private void ShowBackground(string assetId)
    {
        var entry = backgrounds.Find(x => x.Id == assetId);

        if (entry == null || entry.Sprite == null)
        {
            Debug.LogWarning($"Background not found: {assetId}");
            return;
        }

        backgroundView.Show(entry.Sprite);
    }

    private void ShowDescription(ScenarioCommandDto command)
    {
        dialogueView.Show(string.Empty, command.Text);
        dialogueLogView.AddDescription(command.Text);
    }
}
