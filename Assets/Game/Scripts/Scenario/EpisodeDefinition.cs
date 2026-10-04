using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EpisodeDefinition",
    menuName = "Scenario/Episode Definition")]
public class EpisodeDefinition : ScriptableObject
{
    [SerializeField] private string episodeId;
    [SerializeField] private List<TextAsset> scenarioJsons;
    [SerializeField] private string endMessage = "END";

    public string EpisodeId => episodeId;
    public IReadOnlyList<TextAsset> ScenarioJsons => scenarioJsons;
    public string EndMessage => endMessage;
}
