using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private BackgroundView backgroundView;
    [SerializeField] private AudioSource voiceAudioSource;

    [SerializeField] private TextAsset scenarioJson;
    [SerializeField] private List<VoiceEntry> voices;
    [SerializeField] private List<BackgroundEntry> backgrounds;

    private List<ScenarioCommandDto> commands;
    private int currentIndex;

    private void Awake()
    {
        var scenarioData = JsonUtility.FromJson<ScenarioDataDto>(scenarioJson.text);

        commands = scenarioData.Commands;

        Debug.Log($"Scenario loaded: {commands?.Count ?? 0} events");
    }

    private void Start()
    {
        currentIndex = 0;
        ProcessCurrentEvent();
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

    private void NextLine()
    {
        if (currentIndex >= commands.Count)
        {
            return;
        }

        var scenarioEvent = commands[currentIndex];

        if (scenarioEvent.Type != "dialogue")
        {
            return;
        }

        MoveToNextEvent();
    }

    private void MoveToNextEvent()
    {
        currentIndex++;
        ProcessCurrentEvent();
    }

    private void ProcessCurrentEvent()
    {
        if (currentIndex >= commands.Count)
        {
            Debug.Log("End of scenario");
            return;
        }

        var scenarioEvent = commands[currentIndex];

        switch (scenarioEvent.Type)
        {
            case "dialogue":
                ShowDialogue(scenarioEvent);
                break;

            case "background":
                ShowBackground(scenarioEvent.AssetId);
                MoveToNextEvent();
                break;

            default:
                Debug.LogWarning($"Unknown scenario event type: {scenarioEvent.Type}");

                MoveToNextEvent();
                break;
        }
    }

    private void ShowDialogue(ScenarioCommandDto scenarioEvent)
    {
        dialogueView.Show(scenarioEvent.Speaker, scenarioEvent.Text);
        PlayVoice(scenarioEvent.Id);
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

    private void PlayVoice(string id)
    {
        voiceAudioSource.Stop();

        var entry = voices.Find(x => x.Id == id);

        if (entry == null || entry.Clip == null)
        {
            return;
        }

        voiceAudioSource.clip = entry.Clip;
        voiceAudioSource.Play();
    }
}
