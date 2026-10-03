using UnityEngine;

public class VisualNovelPlayer : MonoBehaviour
{
    [SerializeField] private EpisodeDefinition episodeDefinition;
    [SerializeField] private ScenarioRunner scenarioRunner;
    [SerializeField] private VoicePlayer voicePlayer;

    private void Awake()
    {
        if (episodeDefinition == null)
        {
            Debug.LogError("Episode Definition is not assigned.");
            return;
        }

        scenarioRunner.Initialize(episodeDefinition.ScenarioJsons);
        voicePlayer.Initialize(episodeDefinition.EpisodeId);
    }
}
