using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private CharacterPlayer characterPlayer;
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private BackgroundPlayer backgroundPlayer;
    [SerializeField] private DialogueLogView dialogueLogView;
    [SerializeField] private VoicePlayer voicePlayer;
    [SerializeField] private BGMPlayer bgmPlayer;
    [SerializeField] private FadeView fadeView;

    [SerializeField] private List<TextAsset> scenarioJsons;

    [SerializeField] private float skipInterval = 0.08f;
    [SerializeField] private float autoDelay = 0.5f;
    [SerializeField] private float descriptionBaseDuration = 1.5f;
    [SerializeField] private float descriptionSecondsPerCharacter = 0.08f;

    private List<ScenarioCommandDto> commands;
    private int currentScenarioIndex;
    private int currentIndex;
    private float skipTimer;

    private bool autoMode;
    private float autoTimer;
    private bool waitingForAutoAdvance;

    private void Awake()
    {
        if (scenarioJsons == null || scenarioJsons.Count == 0)
        {
            Debug.LogError("Scenario JSON is not assigned.");
        }
    }

    private void Start()
    {
        voicePlayer.PlaybackCompleted += OnVoicePlaybackCompleted;

        currentScenarioIndex = 0;
        LoadScenario(currentScenarioIndex);
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

        var autoPressed = Keyboard.current?.aKey.wasPressedThisFrame == true;

        if (autoPressed)
        {
            autoMode = !autoMode;

            if (autoMode && !waitingForAutoAdvance && !voicePlayer.IsPlaying)
            {
                autoTimer = autoDelay;
                waitingForAutoAdvance = true;
            }
            else if (!autoMode)
            {
                waitingForAutoAdvance = false;
            }

            Debug.Log($"Auto mode: {(autoMode ? "ON" : "OFF")}");
        }

        var mouseClicked = Mouse.current?.leftButton.wasPressedThisFrame == true;
        var screenTouched = Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true;
        var spacePressed = Keyboard.current?.spaceKey.wasPressedThisFrame == true;

        if (mouseClicked || screenTouched || spacePressed)
        {
            NextLine();
        }

        UpdateSkip();
        UpdateAuto();
    }

    private void LoadScenario(int scenarioIndex)
    {
        if (scenarioIndex >= scenarioJsons.Count)
        {
            Debug.Log("End of all scenarios");
            return;
        }

        var scenarioJson = scenarioJsons[scenarioIndex];
        var scenarioData =
            JsonUtility.FromJson<ScenarioDataDto>(scenarioJson.text);

        commands = scenarioData.Commands;
        currentIndex = 0;

        Debug.Log(
            $"Scenario loaded: {scenarioJson.name} ({commands?.Count ?? 0} commands)");

        ProcessCurrentCommand();
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

    private void UpdateAuto()
    {
        if (!autoMode || !waitingForAutoAdvance)
        {
            return;
        }

        autoTimer -= Time.unscaledDeltaTime;

        if (autoTimer > 0f)
        {
            return;
        }

        waitingForAutoAdvance = false;
        NextLine();
    }

    private void OnVoicePlaybackCompleted()
    {
        if (!autoMode)
        {
            return;
        }

        autoTimer = autoDelay;
        waitingForAutoAdvance = true;
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
            MoveToNextScenario();
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
                break;

            case "description":
                ShowDescription(scenarioCommand);
                break;

            case "bgm":
                ProcessBgmCommand(scenarioCommand);
                break;

            case "fade":
                ProcessFadeCommand(scenarioCommand);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown scenario command type: {scenarioCommand.Type}");

                MoveToNextCommand();
                break;
        }
    }

    private void MoveToNextScenario()
    {
        currentScenarioIndex++;
        LoadScenario(currentScenarioIndex);
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
        backgroundPlayer.Show(
            assetId,
            MoveToNextCommand);
    }

    private void ShowDescription(ScenarioCommandDto command)
    {
        dialogueView.Show(string.Empty, command.Text);
        dialogueLogView.AddDescription(command.Text);

        if (!autoMode)
        {
            return;
        }

        autoTimer = descriptionBaseDuration + command.Text.Length * descriptionSecondsPerCharacter;
        waitingForAutoAdvance = true;
    }

    private void ProcessBgmCommand(ScenarioCommandDto command)
    {
        switch (command.Action)
        {
            case "play":
                bgmPlayer.Play(command.AssetId);
                break;

            case "stop":
                bgmPlayer.Stop();
                break;

            default:
                Debug.LogWarning($"Unknown BGM action: {command.Action}");
                break;
        }

        MoveToNextCommand();
    }

    private void ProcessFadeCommand(ScenarioCommandDto command)
    {
        switch (command.Action)
        {
            case "out":
                fadeView.FadeOut(
                    command.Duration,
                    MoveToNextCommand);
                break;

            case "in":
                fadeView.FadeIn(
                    command.Duration,
                    MoveToNextCommand);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown fade action: {command.Action}");

                MoveToNextCommand();
                break;
        }
    }

    private void OnDestroy()
    {
        if (voicePlayer != null)
        {
            voicePlayer.PlaybackCompleted -= OnVoicePlaybackCompleted;
        }
    }
}
