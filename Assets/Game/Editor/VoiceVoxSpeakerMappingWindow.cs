using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class VoiceVoxSpeakerMappingWindow : EditorWindow
{
    private const string MappingDirectory = "Assets/Game/Data/VoiceVox";
    private List<string> episodeIds = new();
    private int selectedEpisodeIndex;

    private List<VoiceVoxSpeakerMapping> mappings = new();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Voice/VOICEVOX Speaker Mappings")]
    private static void Open()
    {
        GetWindow<VoiceVoxSpeakerMappingWindow>("VOICEVOX Speaker Mappings");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("VOICEVOX Speaker Mappings", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        int newSelectedEpisodeIndex = EditorGUILayout.Popup(
            "Episode",
            selectedEpisodeIndex,
            episodeIds.ToArray());

        if (newSelectedEpisodeIndex != selectedEpisodeIndex)
        {
            selectedEpisodeIndex = newSelectedEpisodeIndex;
            LoadMappings();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < mappings.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            mappings[i].ScenarioSpeaker = EditorGUILayout.TextField(mappings[i].ScenarioSpeaker ?? string.Empty);

            mappings[i].VoiceVoxCharacter = EditorGUILayout.TextField(mappings[i].VoiceVoxCharacter ?? string.Empty);

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                mappings.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Mapping"))
        {
            mappings.Add(new VoiceVoxSpeakerMapping());
        }

        if (GUILayout.Button("Save"))
        {
            SaveMappings();
        }
    }

    private void OnEnable()
    {
        RefreshEpisodeList();
    }

    private void RefreshEpisodeList()
    {
        if (!Directory.Exists(MappingDirectory))
        {
            episodeIds.Clear();
            mappings.Clear();
            return;
        }

        episodeIds = Directory
            .GetFiles(MappingDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(id => id)
            .ToList();

        selectedEpisodeIndex = 0;

        if (episodeIds.Count > 0)
        {
            LoadMappings();
        }
    }

    private void LoadMappings()
    {
        string episodeId = GetSelectedEpisodeId();

        if (string.IsNullOrWhiteSpace(episodeId))
        {
            return;
        }

        mappings = VoiceVoxSpeakerMappingRepository.LoadList(episodeId);

        Repaint();
    }

    private void SaveMappings()
    {
        string episodeId = GetSelectedEpisodeId();

        VoiceVoxSpeakerMappingRepository.Save(episodeId, mappings);

        AssetDatabase.Refresh();

        Debug.Log($"VOICEVOX speaker mappings saved: {episodeId}");
    }

    private string GetSelectedEpisodeId()
    {
        return episodeIds[selectedEpisodeIndex];
    }
}
