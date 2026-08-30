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

    private List<ScenarioEvent> events;
    private int currentIndex;

    private void Awake()
    {
        var scenarioData = JsonUtility.FromJson<ScenarioData>(scenarioJson.text);

        events = scenarioData.Events;

        Debug.Log($"Scenario loaded: {events?.Count ?? 0} events");
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
        if (currentIndex >= events.Count)
        {
            return;
        }

        var scenarioEvent = events[currentIndex];

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
        if (currentIndex >= events.Count)
        {
            Debug.Log("End of scenario");
            return;
        }

        var scenarioEvent = events[currentIndex];

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

    private void ShowDialogue(ScenarioEvent scenarioEvent)
    {
        var line = new DialogueLine
        {
            Id = scenarioEvent.Id,
            Speaker = scenarioEvent.Speaker,
            Text = scenarioEvent.Text
        };

        dialogueView.Show(line);
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
