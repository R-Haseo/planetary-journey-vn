using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualNovelPlayer : MonoBehaviour
{
    [SerializeField] private EpisodeDefinition episodeDefinition;
    [SerializeField] private ScenarioRunner scenarioRunner;
    [SerializeField] private VoicePlayer voicePlayer;
    [SerializeField] private EndView endView;

    private void Awake()
    {
        if (episodeDefinition == null)
        {
            Debug.LogError("Episode Definition is not assigned.");
            return;
        }

        scenarioRunner.Initialize(episodeDefinition.ScenarioJsons);
        voicePlayer.Initialize(episodeDefinition.EpisodeId);
        endView.Initialize(episodeDefinition.EndMessage, BackToTitle);
    }

    private void BackToTitle()
    {
        SceneManager.LoadScene("Startup");
    }
}
